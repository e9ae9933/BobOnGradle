﻿using BepInEx;
using BobOnGradle.Properties;
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
				//nahida = PxlsLoader.loadCharacterASync("nahida", Properties.Resources.nahida, 64);
				Console.WriteLine("loading2");
				HarmonyFileLog.Enabled = true;
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
				PR pr = Utils.GetNoel();
				//Console.WriteLine($"noel ({pr.x}, {pr.y}) ({pr.x_shifted}, {pr.y_shifted})");
				while (nextTickList.Count > 0)
				{
					Action a = nextTickList.Dequeue();
					a.Invoke();
				}
				for (int i = 0; i < Patches.draw.Count; i++)
				{
					NelEnemy e = Patches.draw.ElementAt(i).Key;
					if (e.destructed)
					{
						Patches.draw.Remove(e);
						i--;
					}
				}
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
		public static int idYelan;
		public static int idNahida;
		public static int idRaidenShogun;
		public static int idAdvanced;
		static bool inited=false;
		public static PxlCharacter nahida;
		[HarmonyPatch(typeof(SceneGame),"Update")]
		[HarmonyPostfix]
		static void init(byte ___t)
		{
			if (nahida == null)
				nahida = PxlsLoader.loadCharacterASync("nahida", Properties.Resources.nahida, 64);
			if (inited)
				return;
			if (___t != 2)
				return;
			inited = true;
			try
			{
				Console.WriteLine("try init");
				inited = true;
				PxlPose icons=nahida.getPoseByName("icons");
				Utils.registerEnhancer(
					"advance",
					0,
					null,
					"高级功能",
					"显示怪物碰撞箱。\n" +
					"显示攻击判定线。（不准确，测试中）\n" +
					"小剑山拼刀辅助。",
					out idAdvanced);
				NelItem item = Utils.registerEnhancer(
					"hutao",
					0,
					null,
					"幽蝶能留一缕芳",
					"诺艾尔战败时立刻治疗所有异常状态，\n" +
					"然后使用一次无副作用的圣光爆发，\n" +
					"并恢复 100 生命值。",
					out idHutao);
				Utils.registerEnhancer(
					"noelle",
					0,
					null,
					"要一尘不染才行",
					"诺艾尔受到的伤害减少魔力值比例的50%；\n" +
					"魔法霰弹伤害额外提高魔力值比例的25%；\n" +
					"此外，每打倒1个敌人，过充槽立刻获得224魔力值。",
					out idNoelle);/*
				Utils.registerEnhancer(
					"yelan",
					0,
					null,
					"取胜者，大小通吃（未实装）",
					"释放圣光爆发后，诺艾尔将进入「运筹帷幄」状态：\n" +
					"诺艾尔的纯白之箭将发射特殊的「破局矢」。\n" +
					"这种箭矢具有与纯白之箭相似的特性，但击中敌人或墙壁时额外产生一个立刻爆炸的能量球，\n" +
					"造成的伤害视为魔法伤害，能造成能量球156%的伤害。\n" +
					"「运筹帷幄」状态至多持续20秒，将在诺艾尔发射5枚纯白之箭后移除。",
					out idYelan);*/
				Utils.registerEnhancer(
					"nahida",
					0,
					null,
					"大辩圆成之实",
					"敌人对诺艾尔造成伤害时，将会获得蕴种印。\n" +
					"诺艾尔的攻击命中处于蕴种印状态下的敌人时，\n" +
					"将对所有处于蕴种印状态下的敌人释放灭净三业·业障除，\n" +
					"基于本次攻击力的200%，造成同类别的伤害。每0.2秒至多触发一次。",
					out idNahida);
				Utils.registerEnhancer(
					"raiden_shogun",
					0,
					null,
					"负愿前行",
					"诺艾尔的魔法霰弹将无视敌人60%的防御力。\n" +
					"持有魔法霰弹时，诺艾尔不会被敌人的攻击打断。",
					out idRaidenShogun);
				//nahida.getPoseByName("trikarma").getSequence(0).getFrame(0).getImageTexture();
				Console.WriteLine("init with no exceptions");
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Console.WriteLine("init complete2");
		}
	}
}