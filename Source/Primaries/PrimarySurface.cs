using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class PrimarySurface
	{
		public readonly List<PrimaryWork> PrimaryWorkList = new List<PrimaryWork>(); //Contains all work types

		private void OnClicked(EventData data)
		{
			for (int i = 0; i < this.PrimaryWorkList.Count; i++)
			{
				PrimaryWork prime = this.PrimaryWorkList[i];

				if (prime.drawRect.Contains(data.mousePosition))
				{
					prime.OnClicked(data);

					return;
				}
			}
		}

		private void OnHover(EventData data)
		{
			for (int i = 0; i < this.PrimaryWorkList.Count; i++)
			{
				PrimaryWork prime = this.PrimaryWorkList[i];

				if (prime.drawRect.Contains(data.mousePosition))
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

		public void DoEventChecks(Rect surfaceRect, EventData data)
		{
			if (surfaceRect.Contains(data.mousePosition))
			{
				if (data.shift && data.type == EventType.MouseDown)
				{
					this.OnClicked(data);
				}

				else
				{
					this.OnHover(data);
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
