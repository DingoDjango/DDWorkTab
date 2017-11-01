using System.Collections.Generic;
using HugsLib;
using HugsLib.Utils;
using RimWorld.Planet;
using Verse;

namespace DD_WorkTab
{
	public class SurfaceManager : GameComponent
	{
		private List<PawnSurface> allPawnSurfaces = new List<PawnSurface>();

		public Dictionary<Pawn, PawnSurface> PawnSurfaceLookup = new Dictionary<Pawn, PawnSurface>(); //Rebuilt on save load

		public PawnSurface GetPawnSurface(Pawn pawn)
		{
			if (!this.PawnSurfaceLookup.TryGetValue(pawn, out PawnSurface surface))
			{
				bool foundPawn = false;

				for (int i = 0; i < this.allPawnSurfaces.Count; i++)
				{
					PawnSurface listSurface = this.allPawnSurfaces[i];

					if (listSurface.pawn == pawn)
					{
						surface = listSurface;
						this.PawnSurfaceLookup[pawn] = listSurface;
						foundPawn = true;

						break;
					}
				}

				if (!foundPawn)
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
