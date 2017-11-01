using HugsLib;
using HugsLib.Settings;
using HugsLib.Utils;
using RimWorld;
using Verse;

namespace DD_WorkTab
{
	public class Controller : ModBase
	{
		public static SurfaceManager GetManager;

		public static PrimarySurface GetPrimaries;

		public static DraggableWork CurrentDraggable;

		public static SettingHandle<bool> UseSounds;

		public static SettingHandle<bool> ShowPrompt;

		public static SettingHandle<bool> VerboseMessages;

		public static bool ColonistStatsOnlyVisibleMap = true; //Unsaved

		public override string ModIdentifier => "DD_WorkTab";

		public override void DefsLoaded()
		{
			UseSounds = this.Settings.GetHandle("UseSounds", "DD_WorkTab_UseSounds_Label".CachedTranslation(), "DD_WorkTab_UseSounds_Desc".CachedTranslation(), true);

			ShowPrompt = this.Settings.GetHandle("ShowPrompt", "DD_WorkTab_ShowPrompt_Label".CachedTranslation(), "DD_WorkTab_ShowPrompt_Desc".CachedTranslation(), true);

			VerboseMessages = this.Settings.GetHandle("VerboseMessages", "DD_WorkTab_VerboseMessages_Label".CachedTranslation(), "DD_WorkTab_VerboseMessages_Desc".CachedTranslation(), false);
		}

		public override void WorldLoaded()
		{
			GetManager = Current.Game.GetComponent<SurfaceManager>();

			Current.Game.playSettings.useWorkPriorities = true;
		}
	}
}
