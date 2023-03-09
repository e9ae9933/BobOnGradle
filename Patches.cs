using HarmonyLib;
using m2d;
using nel;
using PixelLiner;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using XX;

namespace BobOnGradle
{
	public class Patches
	{
		public static bool passBurst = false;
		[HarmonyPatch(typeof(PR),"initDeath")]
		[HarmonyPrefix]
		public static bool initDeath()
		{
			if (Utils.GetNoel().getEH((EnhancerManager.EH)(1 << Plugin.idHutao)))
			{
				PR pr = Utils.GetNoel();
				passBurst = true;
				pr.addNoDamage(m2d.NDMG._BURST_PREVENT, 1f);
				Plugin.instance.scheduleNextTick(() =>
				{
					pr.changeState(PR.STATE.BURST);
					pr.cureHp(99);
					pr.Skill.BurstSel.fineFaintedRatio();
				});
				pr.Ser.CureAll();
				return false;
			}
			return true;
		}
		[HarmonyPatch(typeof(MagicData),"calcBurstFaintedRatio")]
		[HarmonyPrefix]
		public static bool calcBurstFaintedRatio(ref float __result)
		{
			if(passBurst)
			{
				__result = 0;
				return false;
			}
			return true;
		}
		[HarmonyPatch(typeof(PR),"applyHpDamageRatio")]
		[HarmonyPostfix]
		public static void applyHpDamage(PR __instance,ref float __result)
		{
			if(__instance.getEH((EnhancerManager.EH)(1<<Plugin.idNoelle)))
				__result *= (1f-0.5f*__instance.mp_ratio);
		}
		[HarmonyPatch(typeof(PR),"applyHpDamageSimple")]
		[HarmonyPostfix]
		public static void applyToPlayer(PR __instance,int __result,NelAttackInfoBase Atk)
		{
			M2Attackable a=Atk.AttackFrom;
			if(a is NelEnemy)
			{
				if (!__instance.getEH((EnhancerManager.EH)(1 << Plugin.idNahida)))
					return;
				Console.WriteLine("found " + a);
				draw[a as NelEnemy] = new MeshDrawer();
			}
		}
		static float nahidaTicks = 0;
		[HarmonyPatch(typeof(NelEnemy), "applyDamage")]
		[HarmonyPostfix]
		public static void applyToEnemy(NelEnemy __instance, int __result, NelAttackInfo Atk)
		{
			M2Attackable a = Atk.AttackFrom;
			if (a is PR&&draw.ContainsKey(__instance))
			{
				if (!(a as PR).getEH((EnhancerManager.EH)(1 << Plugin.idNahida)))
					return;
				float ticks=(a as PR).Mp.floort;
				if (ticks > nahidaTicks)
				{
					nahidaTicks = ticks + 12;
					Console.WriteLine("try publish");
					Atk.hpdmg_current *= 2;
					foreach (var e in draw.Keys)
					{
						if (e.is_alive)
							e.applyDamage(Atk);
					}
				}
				else if(ticks+12<nahidaTicks)
				{
					nahidaTicks = 0;
				}
			}
		}
		[HarmonyPatch(typeof(NelEnemy),"applyHpDamageRatio")]
		[HarmonyPostfix]
		public static void applyEnemyDamage(AttackInfo Atk,ref float __result)
		{
			if(Atk is NelAttackInfo)
			{
				NelAttackInfo info = Atk as NelAttackInfo;
				if(info.Caster is PR)
				{
					PR pr = info.Caster as PR;
					if (pr.getEH((EnhancerManager.EH)(1 << Plugin.idRaidenShogun)))
					{
						if (__result < 1)
							__result = (float)(1 - 0.4 * (1 - __result));
					}
					if (pr.getEH((EnhancerManager.EH)(1 << Plugin.idNoelle)))
						__result += 0.25f * pr.mp_ratio;
				}
			}
		}
		[HarmonyPatch(typeof(PR),"changeState")]
		[HarmonyPrefix]
		public static bool changeState(PR.STATE _state,PR __instance)
		{
			if (__instance.getEH((EnhancerManager.EH)(1 << Plugin.idRaidenShogun)))
			{
				Console.WriteLine($"{__instance.getCurMagic()}");
				if (4000 <= (int)_state && (int)_state < 4100 && !__instance.isAbsorbState()&&null!=__instance.getCurMagic())
					return false;
			}
			return true;
		}
		[HarmonyPatch(typeof(NelEnemy),"changeStateToDie")]
		[HarmonyPrefix]
		public static void onEnemyDeath(NelEnemy __instance)
		{
			NelAttackInfo info=__instance.DeathAtk;
			if(info != null)
			{
				if(info.Caster is PR)
				{
					PR pr = info.Caster as PR;
					int got=224;
					if (pr.getEH((EnhancerManager.EH)(1 << Plugin.idNoelle)))
						pr.Skill.getOverChargeSlots().getMana(224,ref got);
				}
			}
		}
		public static Dictionary<NelEnemy, MeshDrawer> draw = new();
		public static Material mat=null;
		[HarmonyPatch(typeof(EnemyAnimator),"FnEnRenderBaseInner")]
		[HarmonyPostfix]
		public static void animate(
			ref int draw_id,ref M2RenderTicket Tk, ref MeshDrawer MdOut,ref bool __result,
			NelEnemy ___Mv,Map2d ___Mp,M2RenderTicket ___RtkBuf)
		{
			if (draw_id == 5)
				__result = true;
			if (draw_id == 6)
			{
				if (___Mv.is_alive && draw.ContainsKey(___Mv))
				{
					M2MoverPr aimPr = ___Mv.AimPr;
					if (aimPr != null && aimPr.is_alive)
					{
						MdOut = draw.GetValueSafe(___Mv);
						MdOut.clear();
						Tk.Matrix = ___RtkBuf.Matrix;
						PxlCharacter chara = Plugin.nahida;
						PxlPose pose = chara.getPoseByName("trikarma");
						PxlSequence seq = pose.getSequence(0);
						PxlFrame frame = seq.getFrame(1);
						if (mat == null)
						{
							mat = MTRX.blend2Mtr(BLEND.NORMAL, frame);
						}
						MdOut.activate("trikarma", mat, false, new Color32(255, 255, 255, 255));
						MdOut.initForImg(frame.getLayer(0).Img);
						MdOut.Rect(0, 0, 32, 32);
					}
				}
				__result = true;
			}
		}
		[HarmonyPatch(typeof(M2MovRenderContainer),"RenderWholeMover")]
		[HarmonyPostfix]
		public static void renderWholeMover(ref ProjectionContainer JCon,ref Camera Cam,ref int draw_id, ref List<M2RenderTicket>[] ___AADob)
		{
			List<M2RenderTicket> list = ___AADob[draw_id];
			foreach (M2RenderTicket ticket in list)
			{
				M2Mover mover=ticket.AssignMover;
				Map2d m2d = mover.Mp;
				float clenb = m2d.CLENB;
				Matrix4x4 mat=Matrix4x4.identity;
				if (mover == null)
					continue;
				if (mover.transform != null)
					mat = mover.transform.localToWorldMatrix;
				else
					Console.WriteLine("no transform: " + mover);
				if(mover.getColliderCreator() == null)
				{
					Console.WriteLine("no collider: " + mover);
					continue;
				}
				if(mover.getColliderCreator().Cld==null)
				{
					Console.WriteLine("no cld: " + mover);
					continue;
				}
				Vector2[] v=mover.getColliderCreator().Cld.GetPath(0);
				if(v==null)
				{
					Console.WriteLine("v null: " + mover);
					continue;
				}
				int n = v.Length;
				MeshDrawer md = new MeshDrawer();
				//Console.WriteLine("clenb "+clenb+" mover "+mover);
				md.activate("bounding_box_refresh", MTRX.MtrMeshNormal, false, C32.d2c(0xFFFFFFFFU));
				MTRX.MtrMeshNormal.SetPass(0);
				md.Col = C32.d2c(0xEE66CCFFU);
				for(int i=0;i<n;i++)
				{
					int j = (i + 1) % n;
					md.Line(v[i].x*clenb, v[i].y*clenb, v[j].x*clenb, v[j].y*clenb,3);
				}
				md.Col = C32.d2c(0xEEEE0000U);
				md.Box(0, 0, 8, 8);
				GL.LoadProjectionMatrix(JCon.CameraProjectionTransformed*mat);
				BLIT.RenderToGLImmediate001(md,md.draw_triangle_count);
			}
		}
		[HarmonyPatch(typeof(PR), "applyBurstMpDamage")]
		[HarmonyPrefix]
		public static bool applyBurstMpDamage()
		{
			if(passBurst)
			{
				passBurst = false;
				return false;
			}
			return true;
		}
		[HarmonyPatch(typeof(NelEnemy),"prepareHpMpBarMesh")]
		[HarmonyPrefix]
		public static void prepareBarPre(ref NelEnemy.FLAG ___flags)
		{
			___flags |= NelEnemy.FLAG.FINE_HPMP_BAR | NelEnemy.FLAG.FINE_HPMP_BAR_CREATE;
		}
		[HarmonyPatch(typeof(M2PrSkill),"isParryable")]
		[HarmonyPostfix]
		public static void tryParry(bool __result,float ___parry_t)
		{
			if(__result)
			{
				UILog.Instance.AddAlert("成功的格挡：剩余时间 " + ___parry_t + " tick(s)");
			}
		}
		[HarmonyPatch(typeof(NelEnemy),"prepareHpMpBarMeshInner")]
		[HarmonyPostfix]
		public static void prepareBarPost(MeshDrawer ___MdHpMpBar,NelEnemy __instance,NAI ___Nai)
		{
			if (__instance is NelNUni && !__instance.isOverDrive()&&___Nai!=null)
			{
				if(__instance.AimPr is PR)
					Console.WriteLine("noel x " + __instance.AimPr.x + " y " + __instance.AimPr.y);
				NaTicket ticket=___Nai.getCurTicket();
				if (ticket!=null&&ticket.type==NAI.TYPE.MAG_0&&(ticket.prog==PROG.ACTIVE||ticket.prog==PROG.PROG0))
				{
					float b = -30;
					int leftticks = 0;
					if(ticket.prog==PROG.ACTIVE)
						leftticks += 70-(int)__instance.state_time;
					double dis = Math.Sqrt((__instance.AimPr.x - __instance.x) * (__instance.AimPr.x - __instance.x)
						+ (__instance.AimPr.y - __instance.y) * (__instance.AimPr.y - __instance.y));
					dis -= 0.75;
					int i = 0;
					for(i=0;i<100;i++)
					{
						float num2 = 0.38f * X.NI(0.4f, 1f, X.ZSIN(i, 11f));
						dis -= num2;
						if (dis <= 0) break;
						leftticks++;
					}
					Console.WriteLine("expect " + i);
					leftticks -= 9;
					if (leftticks < 0)
						return;
					___MdHpMpBar.Col = C32.d2c(2281701376U);
					___MdHpMpBar.BoxBL(-43f, b-10f, 87f, 4f, 0f, false);
					if (leftticks >= 9)
					{
						leftticks -= 9;
						___MdHpMpBar.Col = C32.d2c(0xDE00FF00);
						if (leftticks > 60)
							leftticks = 60;
						___MdHpMpBar.Line(-42, b-8, leftticks * 42 * 2 / 60 - 42, b-8, 2);
					}
					else
					{
						___MdHpMpBar.Col = C32.d2c(0xDEFFFF00);
						___MdHpMpBar.Line(-42, b-8, leftticks * 42 * 2 / 9 - 42, b-8, 2);
						if(Utils.GetNoel().get_current_state()!=PR.STATE.PUNCH)
						{
							//Utils.GetNoel().changeState(PR.STATE.PUNCH);
						}
					}
				}
			}
		}
	}
}
