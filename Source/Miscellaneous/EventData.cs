using UnityEngine;

namespace DD_WorkTab
{
	public struct EventData
	{
		public EventType type;

		public Vector2 mousePosition;

		public int button;

		public bool shift;

		public EventData(EventType type, Vector2 mousePosition, int button, bool shift)
		{
			this.type = type;
			this.mousePosition = mousePosition;
			this.button = button;
			this.shift = shift;
		}
	}
}
