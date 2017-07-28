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
			Rect helpRect = new Rect(rect.x, rect.y, Window_WorkTab.spaceForPawnName + Window_WorkTab.spaceForButtons, rect.height);
			Rect guideButtonRect = helpRect.LeftHalf().Rounded().ContractedBy(5f);
			Rect colonistStatsRect = helpRect.RightHalf().Rounded().ContractedBy(5f);

			Text.Anchor = TextAnchor.MiddleCenter;

			if (Widgets.ButtonText(guideButtonRect, "DD_WorkTab_ButtonHowTo".Translate(), true, false, true))
			{
				Find.WindowStack.Add(new Window_UsageGuide());
			}

			if (Widgets.ButtonText(colonistStatsRect, "DD_WorkTab_ButtonColonistStats".Translate(), true, false, true))
			{
				if (Find.VisibleMap.mapPawns.FreeColonistsCount == 0)
				{
					Messages.Message("DD_WorkTab_ButtonColonistStats_NoColonists".Translate(), MessageSound.RejectInput);
				}

				Find.WindowStack.Add(new Window_ColonistStats(Settings.ColonistStatsOnlyVisibleMap));
			}

			Text.Anchor = TextAnchor.UpperLeft; //Reset

			Vector2 positionSetter = new Vector2(helpRect.xMax + (this.standardSpacing * 2f) + (DDUtilities.DraggableTextureWidth / 2f) - DDUtilities.TabScrollPosition.x, rect.center.y);

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
					TooltipHandler.TipRegion(drawRect, draggable.def.GetDraggableTooltip(true, null));
				}

				positionSetter.x += DDUtilities.DraggableTextureWidth + this.standardSpacing;
			}
		}

		public PrimarySurface()
		{
			//Populate the main surface with indices (only needs to be done once realistically)
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
