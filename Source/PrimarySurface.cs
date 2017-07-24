using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	//This surface is responsible for drawing primary DraggableWorkTypes and general buttons at the top of the window
	public class PrimarySurface
	{
		private float standardSpacing = MainTabWindow_Work_DragAndDrop.spaceBetweenTypes;

		private List<DraggableWorkType> children = new List<DraggableWorkType>();

		public List<DraggableWorkType> childrenListForReading //Unused
		{
			get
			{
				return this.children;
			}
		}

		public IEnumerable<DraggableWorkType> childrenSortedByPriority
		{
			get
			{
				return this.children.OrderBy(child => child.priorityIndex);
			}
		}

		public void OnGUI(Rect rect)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			//Help buttons
			Rect helpRect = new Rect(rect.x, rect.y, MainTabWindow_Work_DragAndDrop.spaceForPawnName + MainTabWindow_Work_DragAndDrop.spaceForButtons, rect.height);
			Rect guideButtonRect = helpRect.LeftHalf().Rounded().ContractedBy(5f);
			Rect colonistStatsRect = helpRect.RightHalf().Rounded().ContractedBy(5f);

			Text.Anchor = TextAnchor.MiddleCenter;

			if (Widgets.ButtonText(guideButtonRect, "How-to Guide", true, false, true))
			{
				//Add some help window here with base.OK dianode
			}

			if (Widgets.ButtonText(colonistStatsRect, "Colonist Stats", true, false, true))
			{
				//Add colonist stats window (look to vanilla pawnrecords?)
			}

			Text.Anchor = TextAnchor.UpperLeft; //Reset

			Vector2 positionSetter = new Vector2(helpRect.xMax + this.standardSpacing + (DDUtilities.DraggableTextureWidth / 2f) - DDUtilities.TabScrollPosition.x, rect.center.y);

			foreach (DraggableWorkType draggable in this.childrenSortedByPriority)
			{
				if (!draggable.IsDragging)
				{
					draggable.position = positionSetter;
				}

				Rect drawRect = DDUtilities.RectOnVector(positionSetter, DDUtilities.DraggableSize);

				draggable.DrawStationaryInformation(drawRect, true);

				draggable.OnGUI();

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

				this.children.Add(primeDraggable);

				currentMainTypePriority++;
			}
		}
	}
}
