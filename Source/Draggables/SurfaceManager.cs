using System.Collections.Generic;
using Verse;

namespace DD_WorkTab.Draggables
{
	public class SurfaceManager : GameComponent
	{
		private List<PawnSurface> allPawnSurfaces = new List<PawnSurface>();

		private Dictionary<Pawn, PawnSurface> PawnSurfaceLookup = new Dictionary<Pawn, PawnSurface>(); //Rebuilt on save load

		public PawnSurface GetPawnSurface(Pawn pawn)
		{
			if (!this.PawnSurfaceLookup.TryGetValue(pawn, out PawnSurface surface))
			{
				PawnSurface listSurface = this.allPawnSurfaces.Find(s => s.pawn == pawn);

				if (listSurface != null)
				{
					surface = listSurface;
					this.PawnSurfaceLookup[pawn] = listSurface;
				}

				else
				{
					surface = new PawnSurface(pawn);
					this.PawnSurfaceLookup[pawn] = surface;

					this.allPawnSurfaces.Add(surface);
				}
			}

			return surface;
		}

		public override void ExposeData()
		{
			base.ExposeData();

			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.allPawnSurfaces.RemoveAll(surface => surface.pawn.DestroyedOrNull());
			}

			Scribe_Collections.Look(ref this.allPawnSurfaces, "allPawnSurfaces", LookMode.Deep, new object[0]);
		}

		public SurfaceManager()
		{
		}

		public SurfaceManager(Game game)
		{
		}
	}
}
