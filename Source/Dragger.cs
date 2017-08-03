using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace DD_WorkTab
{
	public class Dragger : WorldComponent
	{
		public static List<PawnSurface> allPawnSurfaces = new List<PawnSurface>();

		public static Dictionary<Pawn, PawnSurface> surfaceLookup = new Dictionary<Pawn, PawnSurface>(); //Rebuilt on save load

		public static DraggableWorkType CurrentDraggable;

		public static bool Dragging
		{
			get
			{
				return CurrentDraggable != null;
			}
		}

		public static PawnSurface GetPawnSurface(Pawn pawn)
		{
			PawnSurface surface;

			if (!surfaceLookup.TryGetValue(pawn, out surface))
			{
				bool foundPawn = false;

				for (int i = 0; i < allPawnSurfaces.Count; i++)
				{
					if (allPawnSurfaces[i].pawn == pawn)
					{
						surface = allPawnSurfaces[i];
						surfaceLookup[pawn] = allPawnSurfaces[i];

						foundPawn = true;
						break;
					}
				}

				if (!foundPawn)
				{
					surface = new PawnSurface(pawn);

					allPawnSurfaces.Add(surface);
					surfaceLookup[pawn] = surface;
				}
			}

			return surface;
		}

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				allPawnSurfaces.RemoveAll(surface => surface.pawn.DestroyedOrNull());
			}

			Scribe_Collections.Look(ref allPawnSurfaces, "allPawnSurfaces", LookMode.Deep, new object[0]);
		}

		public Dragger(World world) : base(world)
		{
		}
	}
}
