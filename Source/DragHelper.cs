using System.Collections.Generic;
using Verse;

namespace DD_WorkTab
{
	public class DragHelper : GameComponent
	{
		public List<WorkTypeSurface> allPawnSurfaces = new List<WorkTypeSurface>();

		// This "list" is a way to temporarily reference and de-reference a DraggableWorkType
		public List<DraggableWorkType> CurrentDraggingObj = new List<DraggableWorkType>();

		public WorkTypeSurface SurfaceForPawn(Pawn pawn)
		{
			WorkTypeSurface surface = this.allPawnSurfaces.Find(wts => wts.attachedPawn == pawn);

			if (surface == null)
			{
				surface = new WorkTypeSurface(pawn);

				surface.ResetChildrenByVanillaPriorities();

				this.allPawnSurfaces.Add(surface);
			}

			return surface;
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref this.allPawnSurfaces, "allPawnSurfaces", LookMode.Deep);
		}

		//Empty constructors
		public DragHelper()
		{
		}

		public DragHelper(Game game)
		{
		}
	}
}
