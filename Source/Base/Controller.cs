using DD_WorkTab.Draggables;
using DD_WorkTab.Primaries;
using DD_WorkTab.Tools;
using HugsLib;
using HugsLib.Settings;
using Verse;

namespace DD_WorkTab.Base
{
	public class Controller : ModBase
	{
		//Saved in XML
		public static SettingHandle<bool> UseSounds;

		public static SettingHandle<bool> ShowPrompts;

		public static SettingHandle<bool> VerboseMessages;

		//Unsaved
		public static bool ColonistSkillsVisibleMap = true;

		public static SurfaceManager GetManager;

		public static PrimarySurface GetPrimaries;

		public static DraggableWork CurrentDraggable;

		public static PawnSurface CopyPrioritiesReference;

		public override string ModIdentifier => "DD_WorkTab";

		public override void DefsLoaded()
		{
			UseSounds = this.Settings.GetHandle("UseSounds", "DD_WorkTab_UseSounds_Label".CachedTranslation(), "DD_WorkTab_UseSounds_Desc".CachedTranslation(), true);

			ShowPrompts = this.Settings.GetHandle("ShowPrompt", "DD_WorkTab_ShowPrompt_Label".CachedTranslation(), "DD_WorkTab_ShowPrompt_Desc".CachedTranslation(), true);

			VerboseMessages = this.Settings.GetHandle("VerboseMessages", "DD_WorkTab_VerboseMessages_Label".CachedTranslation(), "DD_WorkTab_VerboseMessages_Desc".CachedTranslation(), false);
		}

		public override void WorldLoaded()
		{
			GetManager = Current.Game.GetComponent<SurfaceManager>();

			Current.Game.playSettings.useWorkPriorities = true;
		}
	}
}
