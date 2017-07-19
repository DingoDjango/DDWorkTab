using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	//Stripped-down WorkTypeSurface that is responsible for drawing primary DraggableWorkTypes
	public class PrimaryTypesSurface
	{
		private List<DraggableWorkType> children = new List<DraggableWorkType>();

		public Rect currentListRect;

		public IEnumerable<DraggableWorkType> childrenSortedByPriority
		{
			get
			{
				return this.children.OrderBy(child => child.priorityIndex);
			}
		}

		public void OnGUI()
		{
			DragHelper dragger = Current.Game.GetComponent<DragHelper>();

			float workTypeTextureWidth = DDUtilities.WorkTypeTextureSize.x;

			Vector2 draggablePositionSetter = new Vector2(this.currentListRect.x + 10f + (workTypeTextureWidth / 2f), this.currentListRect.center.y);

			foreach (DraggableWorkType draggable in this.childrenSortedByPriority)
			{
				if (!draggable.IsDragging)
				{
					draggable.position = draggablePositionSetter;
				}

				//Static texture (always drawn even when dragging)
				Rect drawRect = DDUtilities.RectOnVector(draggablePositionSetter, DDUtilities.WorkTypeTextureSize);
				GUI.DrawTexture(drawRect, DDUtilities.TextureFromModFolder(draggable.def));
				Widgets.DrawHighlightIfMouseover(drawRect);
				TooltipHandler.TipRegion(drawRect, draggable.def.labelShort);

				draggable.OnGUI();

				draggablePositionSetter.x += workTypeTextureWidth + 10f;
			}
		}

		public PrimaryTypesSurface()
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
