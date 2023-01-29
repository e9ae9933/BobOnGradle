using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobOnGradle
{
	public class Modules
	{
		Dictionary<string,Module> modules= new();
		public Modules()
		{
			modules.Add("hp1", new ModuleHP(150));
			modules.Add("hp2", new ModuleHP(100));
			modules.Add("hp3", new ModuleHP(50));
			modules.Add("hp4", new ModuleHP(1));
			modules.Add("mp1", new ModuleMP(160));
			modules.Add("mp2", new ModuleMP(120));
			modules.Add("mp3", new ModuleMP(80));
		}
		public void enable(string key)
		{
			modules.GetValueSafe(key).enable();
		}
		public void disable(string key)
		{
			modules.GetValueSafe(key).disable();
		}
	}
}
