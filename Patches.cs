using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
		[HarmonyPatch(typeof(PR), "changeState")]
		[HarmonyPrefix]
		public static bool changeState(PR.STATE _state)
		{
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
		[HarmonyPatch(typeof(M2Ser), "Add")]
		[HarmonyPrefix]
		public static bool Add(SER ser)
		{
			/*
			Console.WriteLine("add ser " + ser);
			if(ser==SER.BURST_TIRED||ser==SER.TIRED)
			{
				Console.WriteLine(Environment.StackTrace);
			}*/
			return true;
		}
		public static void tick()
		{
		}
	}
}
