using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_WorkTab : MainTabWindow
	{
		private const float draggableTextureDiameter = DD_Widgets.DraggableTextureDiameter;

		private const float spaceForPawnLabel = DD_Widgets.SpaceForPawnLabel;

		private const float spaceForWorkButtons = DD_Widgets.SpaceForWorkButtons;

		private const float standardSpacing = DD_Widgets.StandardSpacing;

		private const float standardRowHeight = DD_Widgets.StandardRowHeight;

		private static readonly float standardSurfaceWidth = DD_Widgets.StandardSurfaceWidth;

		private Vector2 scrollPosition = Vector2.zero;

		private bool shouldResetDrag = false;

		private IEnumerable<Pawn> mapColonistsCached;

		private int lastColonistCount;

		private int TotalColonists
		{
			get
			{
				int count = Find.VisibleMap.mapPawns.FreeColonistsCount;

				if (count > 0)
				{
					return count;
				}

				return 1;
			}
		}

		private float PreAdjustedWidth => spaceForPawnLabel + spaceForWorkButtons + standardSurfaceWidth + 2 * standardSpacing + 2 * this.Margin + 20f;

		private float PreAdjustedHeight => standardRowHeight * (1 + this.TotalColonists) + 2 * standardSpacing + 2 * this.Margin + 20f;

		private float ListOffset => PreAdjustedWidth > DD_Widgets.MaxWindowWidth ? this.scrollPosition.x : 0f;

		private void RecacheColonists()
		{
			this.lastColonistCount = this.TotalColonists;

			this.mapColonistsCached = Find.VisibleMap.mapPawns.FreeColonists.OrderBy(p => p.CachedPawnLabel()).ToArray();
		}

		private void DrawPriorityIndicators(Rect rect)
		{
			GUI.color = DD_Widgets.IndicatorsColour;

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<= " + "HigherPriority".CachedTranslation());

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "LowerPriority".CachedTranslation() + " =>");

			GUI.color = Color.white; // Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.SetInitialSizeAndPosition();

			if (this.lastColonistCount != this.TotalColonists)
			{
				this.RecacheColonists();
			}

			Text.Font = GameFont.Tiny; //For indicatorsRect height

			Rect indicatorsRect = new Rect(inRect.x + standardSpacing + spaceForPawnLabel + spaceForWorkButtons, inRect.y, inRect.width - 2 * standardSpacing - spaceForPawnLabel - spaceForWorkButtons, Text.LineHeight);
			Rect primaryTypesRect = new Rect(inRect.x - this.ListOffset, indicatorsRect.yMax, inRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - indicatorsRect.height - primaryTypesRect.height);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, spaceForPawnLabel + spaceForWorkButtons + standardSurfaceWidth, lastColonistCount * standardRowHeight);

			this.DrawPriorityIndicators(indicatorsRect);

			PrimaryTypes.DoWorkTabGUI(primaryTypesRect);

			DD_Widgets.DrawBoxOutline(scrollViewBox);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (Pawn pawn in mapColonistsCached)
			{
				PawnSurface surface = DragManager.GetPawnSurface(pawn);

				if (firstPawnDrawn)
				{
					DD_Widgets.DrawListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				Rect pawnLabelRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				Rect disableWorkRect = new Rect(pawnLabelRect.xMax + standardSpacing, currentVerticalPosition + standardSpacing, draggableTextureDiameter, draggableTextureDiameter);
				Rect resetWorkRect = new Rect(disableWorkRect.xMax + standardSpacing, disableWorkRect.y, disableWorkRect.width, disableWorkRect.height);
				Rect draggablesRect = new Rect(pawnLabelRect.xMax + spaceForWorkButtons, currentVerticalPosition, standardSurfaceWidth, standardRowHeight);

				DD_Widgets.DrawPawnLabel(pawnLabelRect, pawn);

				DD_Widgets.Button(ButtonType.DisableWork, surface, disableWorkRect);

				DD_Widgets.Button(ButtonType.ResetWork, surface, resetWorkRect);

				surface.DoWorkTabGUI(draggablesRect, this.scrollPosition); //Check for drag, draw draggables

				currentVerticalPosition += standardRowHeight;
			}

			Widgets.EndScrollView();

			//Check for invalid drop
			if (this.shouldResetDrag)
			{
				DragManager.CurrentDraggable = null;
				this.shouldResetDrag = false;
			}

			if (DragManager.Dragging && Event.current.type != EventType.MouseUp && !Input.GetMouseButton(0))
			{
				this.shouldResetDrag = true;
			}
		}

		public override void PostClose()
		{
			base.PostClose();

			DragManager.CurrentDraggable = null;
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				float width = PreAdjustedWidth;
				float height = PreAdjustedHeight;

				if (PreAdjustedWidth > DD_Widgets.MaxWindowWidth)
				{
					width = DD_Widgets.MaxWindowWidth;
				}

				if (height > DD_Widgets.MaxWindowHeight)
				{
					height = DD_Widgets.MaxWindowHeight;
				}

				return new Vector2(width, height);
			}
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		public Window_WorkTab()
		{
			Current.Game.playSettings.useWorkPriorities = true;
		}
	}
}
