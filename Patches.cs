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
			if(draw_id==7)
			{
				MdOut = new MeshDrawer();
				MdOut.activate("bounding_box_enemy", MTRX.MtrMeshNormal, false, C32.d2c(0xEEEE0000));
				Tk.Matrix = ___Mv.transform.localToWorldMatrix;
				Map2d m2d = ___Mv.Mp;
				float clenb = m2d.CLENB;
				MdOut.Col = C32.d2c(0xEEEEEE00);
				Vector2[] v=___Mv.getColliderCreator().Cld.GetPath(0);
				int n = v.Length;
				for (int i = 0; i < n; i++)
					MdOut.Line(v[i].x*clenb, v[i].y * clenb, v[(i + 1) % n].x * clenb, v[(i + 1) % n].y * clenb, 2);
				float dx = ___Mv.x_shifted - ___Mv.x, dy = ___Mv.y_shifted - ___Mv.y;
				MdOut.Col = C32.d2c(0xEEEE0000);
				for (int i = 0; i < n; i++)
					MdOut.Line((v[i].x + dx) * clenb, (v[i].y + dy) * clenb, (v[(i + 1) % n].x + dx) * clenb, (v[(i + 1) % n].y + dy) * clenb, 2);
				__result = true;
			}
		}
		[HarmonyPatch(typeof(NoelAnimator),"RenderPrepareMesh")]
		[HarmonyPrefix]
		public static bool prepareNoelRender(ref MeshDrawer MdOut,int draw_id,PRNoel ___Pr,ref bool __result)
		{
			if (draw_id == 2)
			{
				MdOut = new MeshDrawer();
				MdOut.activate("bounding_box_noel", MTRX.MtrMeshNormal, false, C32.d2c(0xEE66CCFF));
				Map2d m2d = ___Pr.Mp;
				Console.WriteLine("prepared");
				float clenb = m2d.CLENB;
				Vector2[] v = ___Pr.getColliderCreator().Cld.GetPath(0);
				int n = v.Length;
				for (int i = 0; i < n; i++)
					MdOut.Line(v[i].x * clenb, v[i].y * clenb, v[(i + 1) % n].x * clenb, v[(i + 1) % n].y * clenb, 2);
				float dx=___Pr.x_shifted-___Pr.x, dy=___Pr.y_shifted-___Pr.y;
				MdOut.Col = C32.d2c(0xEEEE0000);
				for (int i = 0; i < n; i++)
					MdOut.Line((v[i].x+dx) * clenb, (v[i].y+dy) * clenb, (v[(i + 1) % n].x+dx) * clenb, (v[(i + 1) % n].y+dy) * clenb, 2);
				__result = true;
				return false;
			}
			return true;
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
