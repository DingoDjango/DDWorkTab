using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace DD_WorkTab
{
	public class DragManager : WorldComponent
	{
		public static List<PawnSurface> AllPawnSurfaces = new List<PawnSurface>();

		public static Dictionary<Pawn, PawnSurface> PawnSurfaceLookup = new Dictionary<Pawn, PawnSurface>(); //Rebuilt on save load

		public static DraggableWorkType CurrentDraggable;

		public static bool Dragging => CurrentDraggable != null;

		public static PawnSurface GetPawnSurface(Pawn pawn)
		{
			if (!PawnSurfaceLookup.TryGetValue(pawn, out PawnSurface surface))
			{
				bool foundPawn = false;

				for (int i = 0; i < AllPawnSurfaces.Count; i++)
				{
					PawnSurface listSurface = AllPawnSurfaces[i];

					if (listSurface.pawn == pawn)
					{
						surface = listSurface;
						PawnSurfaceLookup[pawn] = listSurface;
						foundPawn = true;

						break;
					}
				}

				if (!foundPawn)
				{
					surface = new PawnSurface(pawn);
					PawnSurfaceLookup[pawn] = surface;

					AllPawnSurfaces.Add(surface);
				}
			}

			return surface;
		}

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				AllPawnSurfaces.RemoveAll(surface => surface.pawn.DestroyedOrNull());
			}

			Scribe_Collections.Look(ref AllPawnSurfaces, "AllPawnSurfaces", LookMode.Deep, new object[0]);
		}

		public DragManager(World world) : base(world)
		{
		}
	}
}
