using System.Collections.Generic;
using DD_WorkTab.Tools;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Primaries
{
	public class PrimarySurface
	{
		public readonly List<PrimaryWork> PrimaryWorkList = new List<PrimaryWork>(); //Contains all work types

		private PrimaryWork PrimaryOnMousePosition => this.PrimaryWorkList.Find(p => p.drawRect.Contains(Event.current.mousePosition));

		private void OnClicked()
		{
			this.PrimaryOnMousePosition?.OnClicked();
		}

		private void OnHover()
		{
			PrimaryWork primary = this.PrimaryOnMousePosition;

			if (primary != null)
			{
				primary.OnHover(primary.drawRect, false);
			}
		}

		public void DrawSurface(Rect rect)
		{
			Vector2 positionSetter = new Vector2(rect.x + 2f * Utilities.ShortSpacing + Utilities.DraggableDiameter / 2f, rect.center.y);

			for (int i = 0; i < this.PrimaryWorkList.Count; i++)
			{
				PrimaryWork primary = this.PrimaryWorkList[i];

				primary.drawRect = positionSetter.ToWorkRect();

				primary.DrawTexture(primary.drawRect);

				positionSetter.x += Utilities.DraggableDiameter + Utilities.ShortSpacing;
			}
		}

		public void DoWorkTabEventChecks(Rect rect)
		{
			if (rect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.Repaint)
				{
					this.OnHover();
				}

				else if (Event.current.type == EventType.MouseDown)
				{
					this.OnClicked();
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
