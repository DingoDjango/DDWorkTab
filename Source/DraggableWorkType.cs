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

		public void OnGUI()
		{
			Rect dragRect = this.position.ToDraggableRect();

			//Draw texture on draggable position
			this.DrawDraggableTexture(dragRect);

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

		public void DrawDraggableTexture(Rect drawRect)
		{
			Texture2D draggableTex = DDUtilities.TextureFromModFolder(this.def);

			if (!Dragger.Dragging && Mouse.IsOver(drawRect))
			{
				GUI.color = DDUtilities.HighlightColour;
			}

			//Draw solid colour texture for primary types
			else if (this.isPrimaryType)
			{
				GUI.color = DDUtilities.ButtonColour;
			}

			//Draw adjusted colour otherwise
			else
			{
				GUI.color = DDUtilities.DraggableColour.AdjustedForPawnSkills(this.parent.pawn, this.def);
			}

			GUI.DrawTexture(drawRect, draggableTex);

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
