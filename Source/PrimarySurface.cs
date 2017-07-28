using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	//This surface is responsible for drawing primary DraggableWorkTypes and general buttons at the top of the window
	public class PrimarySurface
	{
		private float standardSpacing = Window_WorkTab.spaceBetweenTypes;

		private List<DraggableWorkType> primeDraggables = new List<DraggableWorkType>();

		public List<DraggableWorkType> PrimeDraggablesList
		{
			get
			{
				return this.primeDraggables;
			}
		}

		public IEnumerable<DraggableWorkType> PrimeDraggablesByPriority
		{
			get
			{
				return this.primeDraggables.OrderBy(child => child.priorityIndex);
			}
		}

		public void OnGUI(Rect rect)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			//Help buttons
			Rect helpRect = new Rect(rect.x - DDUtilities.TabScrollPosition.x, rect.y + this.standardSpacing, Window_WorkTab.spaceForPawnName + Window_WorkTab.spaceForButtons - 3f, DDUtilities.DraggableTextureHeight);

			if (Widgets.ButtonText(helpRect, "DD_WorkTab_ButtonColonistStats".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Window_ColonistStats(Settings.ColonistStatsOnlyVisibleMap));
			}

			Vector2 positionSetter = new Vector2(helpRect.xMax + (this.standardSpacing * 2f) + (DDUtilities.DraggableTextureWidth / 2f), rect.center.y);

			foreach (DraggableWorkType draggable in this.PrimeDraggablesByPriority)
			{
				Rect drawRect = positionSetter.ToDraggableRect();

				if (!draggable.IsDragging)
				{
					draggable.position = positionSetter;
				}

				else
				{
					draggable.DrawDraggableTexture(drawRect);
				}

				draggable.OnGUI();

				if (!Dragger.Dragging)
				{
					TooltipHandler.TipRegion(drawRect, draggable.def.GetDraggableTooltip(true, false, null));
				}

				positionSetter.x += DDUtilities.DraggableTextureWidth + this.standardSpacing;
			}
		}

		public PrimarySurface()
		{
			//Populate the main surface with all work types
			int currentMainTypePriority = 1;

			foreach (WorkTypeDef typeDef in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				DraggableWorkType primeDraggable = new DraggableWorkType(null, typeDef, currentMainTypePriority);

				primeDraggable.isPrimaryType = true;

				this.primeDraggables.Add(primeDraggable);

				currentMainTypePriority++;
			}
		}
	}
}
