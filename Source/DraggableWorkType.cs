using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class DraggableWorkType : DraggableObject, IExposable
	{
		public PawnSurface parent;

		public WorkTypeDef def;

		public int priorityIndex;

		public bool isPrimaryType;

		public bool isEmergency
		{
			get
			{
				return this.def.workGiversByPriority.Any(wg => wg.emergency);
			}
		}

		public void OnGUI()
		{
			Rect dragRect = this.position.ToDraggableRect();

			//Draw texture on draggable position
			this.DrawTexture(dragRect, false);

			//Do drag calculations
			this.CheckForDrag(dragRect);

			//Update DragManager if this object is being dragged
			if (this.IsDragging)
			{
				if (!Dragger.Dragging || (Dragger.Dragging && Dragger.CurrentDraggingObj[0] != this))
				{
					Dragger.CurrentDraggingObj.Clear();
					Dragger.CurrentDraggingObj.Add(this);
				}
			}
		}

		public void DrawTexture(Rect drawRect, bool statsWindow)
		{
			if (!statsWindow || (statsWindow && this.isPrimaryType))
			{
				Widgets.DrawHighlightIfMouseover(drawRect);
			}

			DDUtilities.DrawOutline(drawRect, this.isEmergency);

			//Adjust colour based on isPrimary, pawn skills, work type
			GUI.color = this.GetDraggableColour();

			GUI.DrawTexture(drawRect.ContractedBy(2f), this.GetDraggableTexture());

			GUI.color = Color.white; //Reset
		}

		#region Constructors
		public DraggableWorkType(PawnSurface surface)
		{
			this.parent = surface;
		}

		public DraggableWorkType(PawnSurface surface, WorkTypeDef wTypeDef, int priority)
		{
			this.parent = surface;
			this.def = wTypeDef;
			this.priorityIndex = priority;
		}

		public DraggableWorkType(PawnSurface surface, WorkTypeDef wTypeDef, Vector2 position)
		{
			this.parent = surface;
			this.def = wTypeDef;
			this.position = position;
		}
		#endregion

		public void ExposeData()
		{
			Scribe_Defs.Look(ref this.def, "def");

			Scribe_Values.Look(ref this.priorityIndex, "priorityIndex");
		}
	}
}
