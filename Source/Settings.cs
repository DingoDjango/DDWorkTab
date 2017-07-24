using Verse;

namespace DD_WorkTab
{
	public class Settings : ModSettings
	{
		public static bool ShowPrompt = true;

		public override void ExposeData()
		{
			base.ExposeData();

			Scribe_Values.Look(ref ShowPrompt, "ShowPrompt", true);
		}
	}
}
