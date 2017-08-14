using Verse;

namespace DD_WorkTab
{
	public class DD_Settings : ModSettings
	{
		public static bool UseSounds = true;

		public static bool ShowPrompt = true;

		public static bool MessageOnDraggableRemoval = false;

		public static bool ColonistStatsOnlyVisibleMap = true; //Unsaved

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref UseSounds, "UseSounds", true);

			Scribe_Values.Look(ref ShowPrompt, "ShowPrompt", true);

			Scribe_Values.Look(ref MessageOnDraggableRemoval, "MessageOnDraggableRemoval", false);
		}
	}
}
