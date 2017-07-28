using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace DD_WorkTab
{
	public static class Dragger
	{
		public static List<PawnSurface> allPawnSurfaces = new List<PawnSurface>();

		public static List<DraggableWorkType> CurrentDraggingObj = new List<DraggableWorkType>(); //Used to temporarily reference a DraggableWorkType

		public static bool Dragging
		{
			get
			{
				return CurrentDraggingObj.Count > 0;
			}
		}

		public static PawnSurface SurfaceForPawn(Pawn pawn)
		{
			PawnSurface surface = allPawnSurfaces.Find(s => s.pawn == pawn);

			if (surface == null)
			{
				surface = new PawnSurface(pawn);

				allPawnSurfaces.Add(surface);
			}

			return surface;
		}
	}
}
