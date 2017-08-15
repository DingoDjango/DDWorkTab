﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public abstract class DD_Window : MainTabWindow
	{
		protected const float draggableTextureDiameter = DD_Widgets.DraggableTextureDiameter;

		protected const float spaceForPawnLabel = DD_Widgets.SpaceForPawnLabel;

		protected const float spaceForWorkButtons = DD_Widgets.SpaceForWorkButtons;

		protected const float standardSpacing = DD_Widgets.StandardSpacing;

		protected const float standardRowHeight = DD_Widgets.StandardRowHeight;

		protected static readonly float pawnSurfaceWidth = DD_Widgets.PawnSurfaceWidth;

		protected Vector2 scrollPosition = Vector2.zero;

		protected Vector2 eventMousePosition;

		protected Vector2 listMousePosition;

		protected float horizontalOffset = 0f;

		protected Map currentMap = Find.VisibleMap;

		protected IEnumerable<PawnSurface> cachedPawnSurfaces;

		protected int cachedColonistCount = 1;

		protected bool mustRecacheColonists = true;

		protected abstract float NaturalWindowWidth();

		protected abstract float NaturalWindowHeight();

		protected abstract int GetColonistCount();

		protected abstract IEnumerable<PawnSurface> CurrentSurfacesList();

		public override void PreOpen()
		{
			if (this.cachedColonistCount != this.GetColonistCount())
			{
				this.mustRecacheColonists = true;
			}
		}

		public override void WindowOnGUI()
		{
			Map visibleMap = Find.VisibleMap;

			if (this.currentMap != visibleMap)
			{
				this.mustRecacheColonists = true;

				this.currentMap = visibleMap;
			}

			if (this.mustRecacheColonists)
			{
				this.mustRecacheColonists = false;

				this.cachedColonistCount = this.GetColonistCount();

				this.cachedPawnSurfaces = this.CurrentSurfacesList().ToArray();
			}

			base.WindowOnGUI();
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.SetInitialSizeAndPosition();

			this.eventMousePosition = Event.current.mousePosition;
			this.listMousePosition = eventMousePosition + this.scrollPosition;
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				float width = NaturalWindowWidth();
				float height = NaturalWindowHeight();
				float maxWidth = DD_Widgets.MaxWindowWidth;
				float maxHeight = DD_Widgets.MaxWindowHeight;

				if (width > maxWidth)
				{
					width = maxWidth;
					this.horizontalOffset = this.scrollPosition.x;
				}

				if (height > maxHeight)
				{
					height = maxHeight;
				}

				return new Vector2(width, height);
			}
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		public DD_Window()
		{
			this.layer = WindowLayer.GameUI;
			this.closeOnClickedOutside = true;
			this.preventCameraMotion = false;
		}
	}
}
