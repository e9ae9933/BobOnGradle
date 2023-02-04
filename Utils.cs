using nel;
using PixelLiner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XX;

namespace BobOnGradle
{
	public class Utils
	{
		public static PRNoel GetNoel()
		{
			return UnityEngine.Object.FindObjectOfType<PRNoel>();
		}
		public static void addText(string key,string value)
		{
			TX t = new(key);
			t.replaceTextContents(value);
			TX.getDefaultFamily().Add(t);
		}
		public static NelItem registerEnhancer(string name,int cost,PxlFrame frame,string title,string desc,out int id)
		{
			Type info = typeof(EnhancerManager);
			FieldInfo fi = info.GetField("AEh", BindingFlags.Static | BindingFlags.NonPublic);
			List<EnhancerManager.Enhancer> list = fi.GetValue(null) as List<EnhancerManager.Enhancer>;
			EnhancerManager.Enhancer enhancer = new(name, frame);
			enhancer.cost = cost;
			NelItem item = NelItem.CreateItemEntry("Enhancer_"+name,
				new NelItem("Enhancer_"+name, 0, 600, 1)
				{
					category = (NelItem.CATEG)10485761U,
					FnGetName = new FnGetItemDetail((item, grade, def) => "强化插槽："+title ),
					FnGetDesc = new((item, grade, def) => "描述"),
					FnGetDetail = new((item, grade, def) => "细节")
				}, false);
			item.value = 1;
			id = list.Count();
			enhancer.ehbit = (EnhancerManager.EH)(1 << id);
			list.Add(enhancer);
			Utils.addText("Enhancer_title_"+name,title);
			Utils.addText("Enhancer_desc_"+name,desc);
			return item;
		}
	}
}
