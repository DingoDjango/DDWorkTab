using RimWorld.Planet;
using Verse;

namespace DD_WorkTab
{
	public class Dragger_SaveManager : WorldComponent
	{
		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				Dragger.allPawnSurfaces.RemoveAll(surface => surface.pawn.DestroyedOrNull());
			}

			Scribe_Collections.Look(ref Dragger.allPawnSurfaces, "allPawnSurfaces", LookMode.Deep, new object[0]);
		}

		public Dragger_SaveManager(World world) : base(world)
		{
		}
	}
}
