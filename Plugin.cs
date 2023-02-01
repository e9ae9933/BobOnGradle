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
		static bool inited=false;
		static PxlCharacter chara;
		[HarmonyPatch(typeof(UIStatus),"fineLoad")]
		[HarmonyPostfix]
		static void init()
		{
			if (inited)
				return;
			try
			{
				inited = true;
				Console.WriteLine("initing chara "+chara+" "+chara.isLoadCompleted());
				PxlPose pose = chara.getPoseByName("hutaoaim");
				PxlSequence seq = pose.getSequence(0);
				PxlFrame f = seq.getFrameByName("hutaoframe");
				Console.WriteLine("layers " + f.countLayers());
				PxlLayer layer = f.getLayerByName("hutao");
				Console.WriteLine("layer " + layer.name + " " + layer.alpha);
				PxlImage img=layer.Img;
				Texture2D tex = (Texture2D)img.get_I();
				Console.WriteLine("texture "+" read "+tex.isReadable);
				File.WriteAllBytes("t.png",tex.EncodeToPNG());
				Console.WriteLine(img.id+" "+img.id2+" "+img.idstr+" "+img.valid);
				Console.WriteLine("f " + f.width + " " + f.height + " name " + f.name);
				NelItem item = Utils.registerEnhancer("hutao", 0, f, "幽蝶能留一缕芳", "诺艾尔战败时立刻治疗所有异常状态，并使用一次无副作用的圣光爆发，并恢复 100 生命值。", out idHutao);
				Utils.GetNoel().NM2D.IMNG.getInventoryEnhancer().Add(item, 1, 0);
				Console.WriteLine("init with no exceptions");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Console.WriteLine("init complete");
		}
		static bool inited1 = false;
		[HarmonyPatch(typeof(XX.MTRX),"init1")]
		[HarmonyPostfix]
		static void init1()
		{
			if (inited1)
				return;
			inited1 = true;
			/*
			chara=PxlsLoader.loadCharacterASync("hutao",File.ReadAllBytes("hutao.pxls"),null,64f);
			*/
			PxlsLoader.texture_unreadable = false;
			chara = new("hutao");
			chara.pixelsPerUnit = 64;
			chara.autoFlipX = true;
			bool p=chara.loadASync(File.ReadAllBytes("hutao.pxls"));
			Console.WriteLine("chara " + chara+" null "+(chara==null)+" suc "+p);
		}
		[HarmonyPatch(typeof(TX),"reloadTx")]
		[HarmonyPostfix]
		static void tx()
		{

		}
		[HarmonyPatch(typeof(ButtonSkinEnhancerRow),"drawIcon")]
		[HarmonyPrefix]
		static void fuckyouhachan(ButtonSkinEnhancerRow __instance,EnhancerManager.Enhancer ___Eh)
		{
			Console.WriteLine(__instance.title + " " + ___Eh.title+" "+___Eh.PF.getImageTexture().GetType());
		}
	}
}