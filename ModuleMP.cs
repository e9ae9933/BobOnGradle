using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobOnGradle
{
	internal class ModuleMP : Module
	{
		int bonus;
		int original;
		public ModuleMP(int bonus)
		{
			this.bonus = bonus;
		}
		public override void onDisable()
		{
			PR pr = GetNoel();
			M2PrSkill.SkillApplyMem m=new M2PrSkill.SkillApplyMem(pr);
			m.maxmp = (int)original;
			m.mp = m.maxmp;
			pr.ApplySkillFixParameter(m);
		}

		public override void onEnable()
		{
			onUpdate();
		}

		public override void onUpdate()
		{
			PR pr = GetNoel();
			M2PrSkill.SkillApplyMem m = new M2PrSkill.SkillApplyMem(pr);
			original = m.maxmp;
			m.maxmp = bonus;
			m.mp = bonus;
			pr.ApplySkillFixParameter(m);
		}
	}
}
