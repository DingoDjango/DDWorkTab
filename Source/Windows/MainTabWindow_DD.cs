using System.Collections.Generic;
using System.Linq;
using DD_WorkTab.Draggables;
using DD_WorkTab.Tools;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Windows
{
	public abstract class MainTabWindow_DD : MainTabWindow
	{
		protected Vector2 scrollPosition = Vector2.zero;

		protected float horizontalOffset = 0f;

		protected Map currentMap = Find.VisibleMap;

		protected List<PawnSurface> cachedPawnSurfaces;

		protected int cachedColonistCount = 1;

		protected bool mustRecacheColonists = true;

		protected abstract float NaturalWindowWidth();

		protected abstract float NaturalWindowHeight();

		protected abstract int GetColonistCount();

		protected abstract IEnumerable<PawnSurface> GetCachedSurfaces();

		public override void PreOpen()
		{
			this.currentMap = Find.VisibleMap;

			if (this.cachedColonistCount != this.GetColonistCount())
			{
				this.mustRecacheColonists = true;
			}
		}

		public override void WindowOnGUI()
		{
			if (this.mustRecacheColonists)
			{
				this.cachedColonistCount = this.GetColonistCount();

				this.cachedPawnSurfaces = this.GetCachedSurfaces().ToList();

				this.mustRecacheColonists = false;
			}

			base.WindowOnGUI();
		}

		public override Vector2 InitialSize
		{
			get
			{
				float width = this.NaturalWindowWidth();
				float height = this.NaturalWindowHeight();
				float maxWidth = Utilities.MaxWindowWidth;
				float maxHeight = Utilities.MaxWindowHeight;

				if (width > maxWidth)
				{
					width = maxWidth;
					this.horizontalOffset = this.scrollPosition.x; //Using scrollPosition.x directly leads to a glitch when scrolling "past" the bottom of the scroll view
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

		public MainTabWindow_DD()
		{
			this.layer = WindowLayer.GameUI;
			this.preventCameraMotion = false;
		}
	}
}
