using System.Collections.Generic;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Primaries
{
	public class PrimarySurface
	{
		public readonly List<PrimaryWork> PrimaryWorkList = new List<PrimaryWork>(); //Contains all work types

		private void OnClicked()
		{
			for (int i = 0; i < this.PrimaryWorkList.Count; i++)
			{
				PrimaryWork prime = this.PrimaryWorkList[i];

				if (prime.drawRect.Contains(Event.current.mousePosition))
				{
					prime.OnClicked();

					return;
				}
			}
		}

		private void OnHover()
		{
			for (int i = 0; i < this.PrimaryWorkList.Count; i++)
			{
				PrimaryWork prime = this.PrimaryWorkList[i];

				if (prime.drawRect.Contains(Event.current.mousePosition))
				{
					prime.OnHover();

					return;
				}
			}
		}

		public void DrawPrimaryDraggables(Rect rect)
		{
			Vector2 positionSetter = new Vector2(rect.x + 2f * Utilities.ShortSpacing + Utilities.DraggableTextureDiameter / 2f, rect.center.y);

			for (int i = 0; i < this.PrimaryWorkList.Count; i++)
			{
				PrimaryWork primary = this.PrimaryWorkList[i];

				primary.drawRect = positionSetter.ToDraggableRect();

				primary.DrawTexture(primary.drawRect);

				positionSetter.x += Utilities.DraggableTextureDiameter + Utilities.ShortSpacing;
			}
		}

		public void DoEventChecks(Rect surfaceRect)
		{
			if (surfaceRect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.MouseDown)
				{
					this.OnClicked();
				}

				else
				{
					this.OnHover();
				}
			}
		}

		public PrimarySurface()
		{
			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				PrimaryWork primaryWork = new PrimaryWork(def);

				this.PrimaryWorkList.Add(primaryWork);
			}
		}
	}
}
