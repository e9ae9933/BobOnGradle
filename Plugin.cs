using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BobOnGradle
{
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		public GUI gui;
		public Modules modules;
		private void Awake()
		{
			try
			{
				Console.WriteLine("loading");
				HarmonyFileLog.Enabled = true;

				Logger.LogInfo("loading gui");

				Process.Start("cmd.exe","/c echo %cd% > cd.txt");

				Logger.LogInfo(System.Environment.CurrentDirectory);
				Process process = new();
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.FileName = @"E:\BobOnGradle\GUI.exe";
				process.Start();
				modules = new();
				gui = new GUI(this, process);


				// Plugin startup logic
				Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

				/*
				double r = nel.M2PrSkill.COMET_RADIUS;
				Harmony.CreateAndPatchAll( typeof( Plugin ) );*/
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
		private void Update()
		{
			//Console.WriteLine("updating");
			gui.update();
			//Console.WriteLine("updated");
			//Console.WriteLine("update "+DateTime.UtcNow.Ticks);
			/*
			if (time >= 0)
			{
				time--;
				if(time<0 )
				{
					UILog.Instance.AddAlert("结束效果：降魔·护法夜叉");
				}
			}*/
		}
		public static int time = -1;
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(MGContainer), "CircleCast")]
		public static IEnumerable<CodeInstruction> CircleCast(IEnumerable<CodeInstruction> instructions)
		{
			Console.WriteLine("checking");
			List<CodeInstruction> list= new List<CodeInstruction>();
			list.Add(new CodeInstruction(OpCodes.Ldarg_0));
			for (int i = 1; i <= 4; i++)
				list.Add(new CodeInstruction(OpCodes.Ldarga_S, i));
			list.Add(new CodeInstruction(OpCodes.Call, typeof(Plugin).GetMethod("onAttack")));
			foreach (CodeInstruction instr in instructions)
			{
				if(instr.opcode==OpCodes.Ret)
				{
					list.Add(new CodeInstruction(OpCodes.Ldarg_0));
					for (int i = 1; i <= 4; i++)
						list.Add(new CodeInstruction(OpCodes.Ldarga_S, i));
					list.Add(new CodeInstruction(OpCodes.Call, typeof(Plugin).GetMethod("postAttack")));
				}
				list.Add(instr);
				if (instr.opcode == OpCodes.Stfld && (instr.operand as FieldInfo).Name.Contains("_apply_knockback_current"))
				{
					Console.WriteLine("patching");
					list.Add(new CodeInstruction(OpCodes.Call, typeof(Plugin).GetMethod("onHit")));
				}
			}
			return list;
		}
		static int num = 0;
		public static void onHit()
		{
			Console.WriteLine("onHit");
			num++;
		}
		public static void onAttack(MGContainer container,ref MagicItem mg,ref M2Ray ray,ref NelAttackInfo atk,ref HITTYPE hittype)
		{
			Console.WriteLine("onAttack "+mg.Caster+" "+mg+" "+atk);
			if(mg.kind==MGKIND.PR_COMET)
			{

			}
			num = 0;
		}
		public static void postAttack(MGContainer container, ref MagicItem mg, ref M2Ray ray, ref NelAttackInfo atk, ref HITTYPE hittype)
		{
			Console.WriteLine("postAttack " + mg.Caster + " " + mg + " " + atk);
			if(num>=2&&mg.kind==MGKIND.PR_COMET&&time<0)
			{
				time = 10 * 60;
				int d = 28*8;
				(mg.Caster as PR).Skill.getOverChargeSlots().getMana(d, ref d);
				UILog.Instance.AddAlert("触发效果：降魔·护法夜叉");
			}
		}
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(M2PrSkill),"runState")]
		public static IEnumerable<CodeInstruction> patchState(IEnumerable<CodeInstruction> instructions)
		{
			int d = -1;
			foreach(var instruction in instructions)
			{
				yield return instruction;
				if(instruction.opcode==OpCodes.Ldstr&&instruction.operand=="attack_misogi3")
				{
					d = 3;
				}
				if(d==0)
				{
					Console.WriteLine("patch after " + instruction);
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return new CodeInstruction(OpCodes.Ldc_I4_0);
					yield return new CodeInstruction(OpCodes.Ldnull);
					yield return new CodeInstruction(OpCodes.Callvirt, typeof(M2PrSkill).GetMethod("executeSmallAttack",BindingFlags.NonPublic|BindingFlags.Instance));
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Dup);
					yield return new CodeInstruction(OpCodes.Ldfld, typeof(MagicItem).GetField("sz"));
					yield return new CodeInstruction(OpCodes.Ldc_R4, 7.5f);
					yield return new CodeInstruction(OpCodes.Mul);
					yield return new CodeInstruction(OpCodes.Stfld, typeof(MagicItem).GetField("sz"));
					yield return new CodeInstruction(OpCodes.Ldc_R4, 1f);
					yield return new CodeInstruction(OpCodes.Callvirt, typeof(MagicItem).GetMethod("run"));
					yield return new CodeInstruction(OpCodes.Pop);
				}
				d--;
			}
		}
		[HarmonyDebug]
		[HarmonyTranspiler]
		[HarmonyPatch(typeof(M2PrSkill),"publishShotgunHit")]
		public static IEnumerable<CodeInstruction> patchPublish(IEnumerable<CodeInstruction> instructions,ILGenerator generator)
		{
			bool r = true;
			for(int i=0;i<instructions.Count();i++)
			{
				CodeInstruction instruction=instructions.ElementAt(i);
				yield return instruction;
				if(instruction.opcode==OpCodes.Stloc_2&&r)
				{
					System.Reflection.Emit.Label label =generator.DefineLabel();
					yield return new CodeInstruction(OpCodes.Call, typeof(Plugin).GetMethod("cancelPublish"));
					yield return new CodeInstruction(OpCodes.Brfalse_S,label);
					yield return new CodeInstruction(OpCodes.Ldc_I4_1);
					yield return new CodeInstruction(OpCodes.Ret);
					instructions.ElementAt(i+1).labels.Add(label);
					Console.WriteLine("patch 2");
					r = false;
				}
			}
		}
		public static bool cancelPublish()
		{
			Console.WriteLine("publish");
			//return true;
			return time >= 0;
			return true;
		}
	}
}