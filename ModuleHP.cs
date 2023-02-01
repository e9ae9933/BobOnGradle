using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BobOnGradle
{
	internal class ModuleHP : Module
	{
		int bonus;
		int original;
		public ModuleHP(int bonus)
		{
			this.bonus = bonus;
		}
		public override void onDisable()
		{
			PR pr = Utils.GetNoel();
			M2PrSkill.SkillApplyMem m=new M2PrSkill.SkillApplyMem(pr);
			m.maxhp = (int)original;
			m.hp = m.maxhp;
			pr.ApplySkillFixParameter(m);
		}

		public override void onEnable()
		{
			onUpdate();
		}

		public override void onUpdate()
		{
			PR pr = Utils.GetNoel();
			M2PrSkill.SkillApplyMem m = new M2PrSkill.SkillApplyMem(pr);
			original = m.maxhp;
			m.maxhp = bonus;
			m.hp = bonus;
			pr.ApplySkillFixParameter(m);
		}
	}
}
