using BepInEx;
using HarmonyLib;
using HarmonyLib.Tools;
using m2d;
using nel;
using PixelLiner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using UnityEngine;
using XX;

namespace BobOnGradle
{
	[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin
	{
		public static Plugin instance=null;
		public GUI gui;
		public Modules modules;
		Queue<Action> nextTickList = new();

		private void Awake()
		{
			System.Diagnostics.Debugger.Launch();
			instance = this;
			try
			{
				Console.WriteLine("loading");
				HarmonyFileLog.Enabled = true;
				/*
				Logger.LogInfo("loading gui");

				Process.Start("cmd.exe","/c echo %cd% > cd.txt");

				Logger.LogInfo(System.Environment.CurrentDirectory);
				Process process = new();
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.FileName = @"E:\BobOnGradle\GUI.exe";
				process.Start();
				modules = new();
				gui = new GUI(this, process);
				*/
				Harmony.CreateAndPatchAll(typeof(Patches));
				Harmony.CreateAndPatchAll(this.GetType());

				Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
		}
		private void Update()
		{

			try
			{
				while (nextTickList.Count > 0)
				{
					Action a = nextTickList.Dequeue();
					a.Invoke();
				}
				Patches.tick();
			}
			catch(Exception e)
			{
				Logger.LogError(e);
			}
		}
		public void scheduleNextTick(Action a)
		{
			nextTickList.Enqueue(a);
		}
		public static int idHutao;
		public static int idNoelle;
		static bool inited=false;
		[HarmonyPatch(typeof(SceneGame),"Update")]
		[HarmonyPostfix]
		static void init(byte ___t)
		{
			if (inited)
				return;
			if (___t != 2)
				return;
			inited = true;
			try
			{
				inited = true;
				NelItem item = Utils.registerEnhancer(
					"hutao",
					0,
					null,
					"幽蝶能留一缕芳",
					"诺艾尔战败时立刻治疗所有异常状态，\n" +
					"并使用一次无副作用的圣光爆发，\n" +
					"且恢复 100 生命值。",
					out idHutao);
				Utils.registerEnhancer(
					"noelle",
					0,
					null,
					"要一尘不染才行",
					"诺艾尔获得魔力值50%的防御力；\n" +
					"魔法霰弹额外提高诺艾尔防御力50%的攻击力；\n" +
					"此外，每打倒1个敌人，过充槽立刻获得224魔力值。",
					out idNoelle);
				//Utils.GetNoel().NM2D.IMNG.getInventoryEnhancer().Add(item, 1, 0);
				SceneGame.prepareM2DObject();
				Console.WriteLine("init with no exceptions");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Console.WriteLine("init complete");
		}
		static bool inited1 = false;
		[HarmonyPatch(typeof(TX),"reloadTx")]
		[HarmonyPostfix]
		static void tx()
		{

		}
	}
}