using DD_WorkTab.Miscellaneous;
using UnityEngine;

namespace DD_WorkTab.Draggables
{
	/* Based on the work of Emil Johansen aka AngryAnt
	* https://github.com/AngryAnt */

	public abstract class DraggableObject
	{
		protected bool draggingNow;

		protected Vector2 dragOffsetFromMouse;

		public Vector2 position;

		public Rect dragRect;

		public bool DraggingNow => this.draggingNow;

		public abstract void OnClicked(EventData eventData);

		public abstract void OnDrag(EventData eventData);

		public abstract void OnDrop();

		public abstract void OnHover();
	}
}
