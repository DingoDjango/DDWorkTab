using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_WorkTab : MainTabWindow
	{
		private static float spaceForPawnLabel = DDUtilities.spaceForPawnLabel;

		private static float spaceForWorkButtons = DDUtilities.spaceForWorkButtons;

		private static float standardSpacing = DDUtilities.standardSpacing;

		private static float standardRowHeight = DDUtilities.standardRowHeight;

		private static float standardSurfaceWidth = DDUtilities.standardSurfaceWidth;

		private float preAdjustedWidth => spaceForPawnLabel + spaceForWorkButtons + standardSurfaceWidth + 2 * standardSpacing + 2 * this.Margin + 20f;

		private float preAdjustedHeight => standardRowHeight * (1 + this.totalColonists) + 2 * standardSpacing + 2 * this.Margin + 20f;

		private static float maxAllowedWidth = (float)UI.screenWidth - 10f;

		private static float maxAllowedHeight = (float)UI.screenHeight * 0.75f;

		private float listOffset => preAdjustedWidth > maxAllowedWidth ? this.scrollPosition.x : 0f;

		private PrimarySurface primarySurface = new PrimarySurface();

		private Vector2 scrollPosition = Vector2.zero;

		private bool shouldResetDrag = false;

		private IEnumerable<Pawn> mapColonistsCached;

		private int lastColonistCount;

		private int totalColonists
		{
			get
			{
				int count = Find.VisibleMap.mapPawns.FreeColonistsCount;

				if (count > 0)
				{
					return count;
				}

				else return 1;
			}
		}

		private void RecacheColonists()
		{
			this.lastColonistCount = this.totalColonists;

			this.mapColonistsCached = Find.VisibleMap.mapPawns.FreeColonists.OrderBy(p => DDUtilities.GetLabelForPawn(p)).ToArray();
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			if (this.lastColonistCount != this.totalColonists)
			{
				this.RecacheColonists();
			}

			Text.Font = GameFont.Tiny;
			float spaceForIndicators = Text.LineHeight;

			//General Rects
			Rect indicatorsRect = new Rect(inRect.x + standardSpacing + spaceForPawnLabel + spaceForWorkButtons, inRect.y, inRect.width - 2 * standardSpacing - spaceForPawnLabel - spaceForWorkButtons, spaceForIndicators);
			Rect primarySurfaceRect = new Rect(inRect.x - this.listOffset, indicatorsRect.yMax, inRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primarySurfaceRect.yMax, inRect.width, inRect.height - indicatorsRect.height - primarySurfaceRect.height);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, spaceForPawnLabel + spaceForWorkButtons + standardSurfaceWidth, lastColonistCount * standardRowHeight);

			//Draw priority indicators
			DDUtilities.DrawPriorityIndicators(indicatorsRect);

			//Primary work types
			primarySurface.OnWorkTabGUI(primarySurfaceRect);

			//Draw rect edges
			DDUtilities.DrawOutline(scrollViewBox, false, true);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (Pawn pawn in mapColonistsCached)
			{
				PawnSurface surface = Dragger.GetPawnSurface(pawn);

				//List separator
				if (firstPawnDrawn)
				{
					DDUtilities.DrawListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				Rect surfaceRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, scrollViewInnerRect.width, standardRowHeight);

				//Check for drag, draw draggables
				surface.OnWorkTabGUI(surfaceRect, this.scrollPosition);

				//Increment list y for next pawn
				currentVerticalPosition += standardRowHeight;
			}

			Widgets.EndScrollView();

			//Check for invalid drop
			if (this.shouldResetDrag)
			{
				Dragger.CurrentDraggable = null;
				this.shouldResetDrag = false;
			}

			if (Event.current.type != EventType.MouseUp && Dragger.Dragging && !Input.GetMouseButton(0))
			{
				this.shouldResetDrag = true;
			}
		}

		//General Window settings
		public Window_WorkTab()
		{
			Current.Game.playSettings.useWorkPriorities = true;
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				float width = preAdjustedWidth;
				float height = preAdjustedHeight;

				if (preAdjustedWidth > maxAllowedWidth)
				{
					width = maxAllowedWidth;
				}

				if (height > maxAllowedHeight)
				{
					height = maxAllowedHeight;
				}

				return new Vector2(width, height);
			}
		}

		public override void PostClose()
		{
			base.PostClose();

			Dragger.CurrentDraggable = null;
		}
	}
}
