using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	/* Based on the work of Emil Johansen aka AngryAnt
	 * https://github.com/AngryAnt */

	public abstract class DraggableObject
	{
		public Vector2 position;

		private Vector2 dragOffsetVector;

		protected bool draggingNow;

		public bool IsDragging
		{
			get
			{
				return this.draggingNow;
			}
		}

		protected void CheckForDrag(Rect draggingRect)
		{
			if (Event.current.type == EventType.MouseUp)
			{
				this.draggingNow = false;
			}

			else if (DragStarted(draggingRect))
			{
				this.draggingNow = true;
				this.dragOffsetVector = Event.current.mousePosition - this.position;
				Event.current.Use();
			}

			if (draggingNow)
			{
				this.position = Event.current.mousePosition - this.dragOffsetVector;
			}
		}

		//Verse.Mouse
		private static bool DragStarted(Rect dragRect)
		{
			return Event.current.type == EventType.MouseDown
				&& Event.current.button == 0
				&& dragRect.Contains(Event.current.mousePosition)
				&& !Find.WindowStack.MouseObscuredNow
				&& Find.WindowStack.CurrentWindowGetsInput;
		}
	}
}
