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

		private DragManager dragger
		{
			get
			{
				return Current.Game.World.GetComponent<DragManager>();
			}
		}

		public void OnGUI()
		{
			//Draw texture on draggable position
			Rect drawRect = DDUtilities.RectOnVector(this.position, DDUtilities.DraggableSize);

			GUI.DrawTexture(drawRect, DDUtilities.TextureFromModFolder(this.def));

			this.CheckForDrag(drawRect);

			if (this.IsDragging)
			{
				if (!dragger.Dragging || (dragger.Dragging && dragger.CurrentDraggingObj[0] != this))
				{
					dragger.CurrentDraggingObj.Clear();
					dragger.CurrentDraggingObj.Add(this);
				}
			}
		}

		public void DrawStationaryInformation(Rect drawRect, bool isPrimary)
		{
			if (isPrimary) //Used to give the illusion that primary work types are "copied" on GUI
			{
				GUI.DrawTexture(drawRect, DDUtilities.TextureFromModFolder(this.def));
			}

			Widgets.DrawHighlightIfMouseover(drawRect);

			string tooltip = isPrimary ? this.def.labelShort + "\n\n" + this.def.description : this.def.labelShort;

			TooltipHandler.TipRegion(drawRect, tooltip);
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
