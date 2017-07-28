using UnityEngine;

namespace DD_WorkTab
{
	/* Based on the work of Emil Johansen aka AngryAnt
	 * https://github.com/AngryAnt */

	public abstract class DraggableObject
	{
		public Vector2 position;

		protected Vector2 dragOffsetVector;

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

			else if (DDUtilities.MouseLeftClickedRect(draggingRect))
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
	}
}
