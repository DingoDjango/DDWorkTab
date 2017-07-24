using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace DD_WorkTab
{
	public class DragManager : WorldComponent
	{
		public List<PawnSurface> allPawnSurfaces = new List<PawnSurface>();

		public List<DraggableWorkType> CurrentDraggingObj = new List<DraggableWorkType>(); //Used to temporarily reference a DraggableWorkType

		public bool Dragging
		{
			get
			{
				return this.CurrentDraggingObj.Count > 0;
			}
		}

		public PawnSurface SurfaceForPawn(Pawn pawn)
		{
			PawnSurface surface = this.allPawnSurfaces.Find(s => s.pawn == pawn);

			if (surface == null)
			{
				surface = new PawnSurface(pawn);

				this.allPawnSurfaces.Add(surface);
			}

			return surface;
		}

		public override void ExposeData()
		{
			if (Scribe.mode == LoadSaveMode.Saving)
			{
				this.allPawnSurfaces.RemoveAll(surface => surface.pawn.DestroyedOrNull());
			}

			Scribe_Collections.Look(ref this.allPawnSurfaces, "allPawnSurfaces", LookMode.Deep, new object[0]);
		}

		public DragManager(World world) : base(world)
		{
		}
	}
}
