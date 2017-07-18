using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RimWorld;
using UnityEngine;
using UnityEngine.UI;
using Verse;

namespace DD_WorkTab
{
	public class DragHelper : GameComponent
	{
		public List<WorkTypeSurface> allWorkTypeSurfaces = new List<WorkTypeSurface>();

		/* This is not really a list, but a way to temporarily reference and de-reference a WorkTypeIndex
		 * CurrentDraggingObj should always contain only one element */
		public List<DraggableWorkType> CurrentDraggingObj = new List<DraggableWorkType>();

		public WorkTypeSurface SurfaceForPawn(Pawn pawn)
		{
			WorkTypeSurface surface = this.allWorkTypeSurfaces.Find(wts => wts.attachedPawn == pawn);

			if (surface == null)
			{
				List<DraggableWorkType> pawnWorkIndices = new List<DraggableWorkType>();

				surface = new WorkTypeSurface(pawn, pawnWorkIndices);

				int currentPriority = 1;
				foreach (var workType in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
				{
					if (!pawn.story.WorkTypeIsDisabled(workType))
					{
						DraggableWorkType newWorkTypeIndex = new DraggableWorkType(surface, workType, currentPriority);
						surface.AddOrUpdateChild(newWorkTypeIndex);
					}

					currentPriority++;
				}

				this.allWorkTypeSurfaces.Add(surface);
			}

			return surface;
		}

		public override void ExposeData()
		{
			Scribe_Collections.Look(ref this.allWorkTypeSurfaces, "allWorkTypeSurfaces", LookMode.Deep);
		}

		//Constructors
		public DragHelper()
		{
		}

		public DragHelper(Game game)
		{
		}
	}
}
