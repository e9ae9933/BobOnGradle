﻿using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobOnGradle
{
	public abstract class Module
	{
		public abstract void onEnable();
		public abstract void onDisable();
		public abstract void onUpdate();

		public bool enabled;
		public void enable()
		{
			if (enabled) throw new Exception("already enabled");
			onEnable();
			enabled = true;
		}
		public void disable()
		{
			if (!enabled) throw new Exception("already disabled");
			onDisable();
			enabled = false;
		}
	}
}
