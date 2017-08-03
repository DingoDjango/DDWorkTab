using System.Reflection;
using Harmony;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Controller : Mod
	{
		public Controller(ModContentPack content) : base(content)
		{
			HarmonyInstance harmony = HarmonyInstance.Create("dingo.rimworld.dd_worktab");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			this.GetSettings<Settings>();
		}

		public override void DoSettingsWindowContents(Rect inRect)
		{
			Listing_Standard list = new Listing_Standard();
			list.ColumnWidth = inRect.width / 2f;
			list.Begin(inRect);

			list.Gap(20f);

			list.CheckboxLabeled("DD_WorkTab_UseSounds_Label".TranslateFast(), ref Settings.UseSounds, "DD_WorkTab_UseSounds_Desc".TranslateFast());

			list.Gap(20f);

			list.CheckboxLabeled("DD_WorkTab_ShowPrompt_Label".TranslateFast(), ref Settings.ShowPrompt, "DD_WorkTab_ShowPrompt_Desc".TranslateFast(new string[]
				{
					"DD_WorkTab_ButtonDisableAll_Tooltip".TranslateFast(),
					"DD_WorkTab_ButtonResetVanilla_Tooltip".TranslateFast()
				}));

			list.Gap(20f);

			list.CheckboxLabeled("DD_WorkTab_MessageOnDraggableRemoval_Label".TranslateFast(), ref Settings.MessageOnDraggableRemoval, "DD_WorkTab_MessageOnDraggableRemoval_Desc".TranslateFast());

			list.End();
		}

		public override string SettingsCategory()
		{
			return "DD WorkTab";
		}
	}
}
