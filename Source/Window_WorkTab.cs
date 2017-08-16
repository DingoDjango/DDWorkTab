using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_WorkTab : DD_Window
	{
		private bool invalidDrag = false;

		protected override float NaturalWindowWidth()
		{
			return spaceForPawnLabel + spaceForWorkButtons + pawnSurfaceWidth + 2 * standardSpacing + 2 * StandardMargin + 20f;
		}

		protected override float NaturalWindowHeight()
		{
			return standardRowHeight * (1 + this.cachedColonistCount) + 2 * standardSpacing + 2 * StandardMargin + 20f;
		}

		protected override int GetColonistCount()
		{
			int count = this.currentMap.mapPawns.FreeColonistsCount;

			if (count == 0)
			{
				return 1;
			}

			return count;
		}

		protected override IEnumerable<PawnSurface> GetCachedSurfaces()
		{
			foreach (Pawn pawn in this.currentMap.mapPawns.FreeColonists.OrderBy(p => p.CachedPawnLabel()))
			{
				yield return DragManager.GetPawnSurface(pawn);
			}
		}

		private void DrawPriorityIndicators(Rect rect)
		{
			Text.Font = GameFont.Tiny;
			GUI.color = DD_Widgets.IndicatorsColour;

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<= " + "HigherPriority".CachedTranslation());

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "LowerPriority".CachedTranslation() + " =>");

			Text.Font = GameFont.Small; //Reset
			GUI.color = Color.white; //Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			Rect indicatorsRect = new Rect(inRect.x + standardSpacing + spaceForPawnLabel + spaceForWorkButtons, inRect.y, inRect.width - 2 * standardSpacing - spaceForPawnLabel - spaceForWorkButtons, DD_Widgets.TinyTextLineHeight);
			Rect primaryTypesRect = new Rect(inRect.x - this.horizontalOffset, indicatorsRect.yMax, inRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - indicatorsRect.height - primaryTypesRect.height);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, spaceForPawnLabel + spaceForWorkButtons + pawnSurfaceWidth, this.cachedColonistCount * standardRowHeight);

			this.DrawPriorityIndicators(indicatorsRect);

			PrimaryTypes.DoWorkTabGUI(primaryTypesRect, this.eventMousePosition);

			DD_Widgets.BoxOutline(scrollViewBox);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (PawnSurface surface in this.cachedPawnSurfaces)
			{
				if (firstPawnDrawn)
				{
					DD_Widgets.ListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				Rect pawnLabelRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				Rect disableWorkRect = new Rect(pawnLabelRect.xMax + standardSpacing, currentVerticalPosition + standardSpacing, draggableTextureDiameter, draggableTextureDiameter);
				Rect resetWorkRect = new Rect(disableWorkRect.xMax + standardSpacing, disableWorkRect.y, disableWorkRect.width, disableWorkRect.height);
				Rect draggablesRect = new Rect(pawnLabelRect.xMax + spaceForWorkButtons, currentVerticalPosition, pawnSurfaceWidth, standardRowHeight);

				DD_Widgets.PawnLabel(pawnLabelRect, surface.pawn, this.listMousePosition);

				DD_Widgets.Button(ButtonType.DisableWork, surface, disableWorkRect, this.listMousePosition);

				DD_Widgets.Button(ButtonType.ResetWork, surface, resetWorkRect, this.listMousePosition);

				surface.DoWorkTabGUI(draggablesRect, this.scrollPosition, this.listMousePosition);

				currentVerticalPosition += standardRowHeight;
			}

			Widgets.EndScrollView();

			//Check for invalid drop
			if (this.invalidDrag)
			{
				DragManager.CurrentDraggable = null;
				this.invalidDrag = false;
			}

			if (DragManager.Dragging && Event.current.type != EventType.MouseUp && !Input.GetMouseButton(0))
			{
				this.invalidDrag = true;
			}
		}

		public override void PostClose()
		{
			base.PostClose();

			DragManager.CurrentDraggable = null;
		}

		public Window_WorkTab() : base()
		{
			Current.Game.playSettings.useWorkPriorities = true;
		}
	}
}
