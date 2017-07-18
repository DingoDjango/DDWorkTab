﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class DraggableWorkType : DraggableObject, IExposable
	{
		public WorkTypeSurface parent = null;

		public WorkTypeDef def = null;

		public int priorityIndex = -1;

		public bool isPrimaryType = false;

		public DraggableWorkType()
		{
		}

		public DraggableWorkType(WorkTypeSurface wts)
		{
			this.parent = wts;
		}

		public DraggableWorkType(WorkTypeSurface surface, WorkTypeDef wTypeDef, int priority)
		{
			this.parent = surface;
			this.def = wTypeDef;
			this.priorityIndex = priority;
		}

		public DraggableWorkType(WorkTypeSurface surface, WorkTypeDef wTypeDef, int priority, Vector2 position) : base(position)
		{
			this.parent = surface;
			this.def = wTypeDef;
			this.priorityIndex = priority;
		}

		public void OnGUI()
		{
			//Draw texture on draggable position
			Rect drawRect = DDUtilities.RectOnVector(this.position, DDUtilities.WorkTypeTextureSize);

			GUI.DrawTexture(drawRect, DDUtilities.TextureFromModFolder(this.def));

			this.CheckForDrag(drawRect);

			if (this.IsDragging)
			{
				var dragger = Current.Game.GetComponent<DragHelper>();

				if (dragger.CurrentDraggingObj.Count == 0)
				{
					dragger.CurrentDraggingObj.Add(this);
				}
			}
		}

		public void ExposeData()
		{
			Scribe_Values.Look(ref this.def, "def", null);
			Scribe_Values.Look(ref this.priorityIndex, "priorityIndex", -1);
		}
	}
}