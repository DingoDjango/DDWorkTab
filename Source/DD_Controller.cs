using System.Reflection;
using Harmony;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class DD_Controller : Mod
	{
		public DD_Controller(ModContentPack content) : base(content)
		{
			HarmonyInstance harmony = HarmonyInstance.Create("dingo.rimworld.dd_worktab");

			harmony.PatchAll(Assembly.GetExecutingAssembly());

			this.GetSettings<DD_Settings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard()
			{
				ColumnWidth = inRect.width / 2f
			};

			list.Begin(inRect);

			list.Gap(20f);

			list.CheckboxLabeled("DD_WorkTab_UseSounds_Label".CachedTranslation(), ref DD_Settings.UseSounds, "DD_WorkTab_UseSounds_Desc".CachedTranslation());

			list.Gap(20f);

			list.CheckboxLabeled("DD_WorkTab_ShowPrompt_Label".CachedTranslation(), ref DD_Settings.ShowPrompt, "DD_WorkTab_ShowPrompt_Desc".CachedTranslation());

			list.Gap(20f);

			list.CheckboxLabeled("DD_WorkTab_MessageOnRemoval_Label".CachedTranslation(), ref DD_Settings.MessageOnDraggableRemoval, "DD_WorkTab_MessageOnRemoval_Desc".CachedTranslation());

			list.End();
		}

		public override string SettingsCategory()
		{
			return "Drag and Drop Work Tab";
		}
	}
}
