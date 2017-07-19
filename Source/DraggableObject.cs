using UnityEngine;

namespace DD_WorkTab
{
	public class DraggableObject
	{
		public Vector2 position;

		private Vector2 dragOffsetVector;

		private bool draggingNow = false;

		public DraggableObject()
		{
		}

		public DraggableObject(Vector2 posVector)
		{
			this.position = posVector;
		}

		public bool IsDragging
		{
			get
			{
				return this.draggingNow;
			}
		}

		public void CheckForDrag(Rect draggingRect)
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
