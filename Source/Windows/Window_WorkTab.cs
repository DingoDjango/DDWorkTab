using System.Collections.Generic;
using System.Linq;
using DD_WorkTab.Base;
using DD_WorkTab.Draggables;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Windows
{
	public class Window_WorkTab : MainTabWindow_DD
	{
		protected override float NaturalWindowWidth() => 2f * StandardMargin + 2f * Utilities.ShortSpacing + Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButton + Utilities.PawnSurfaceWidth + Utilities.SpaceForScrollBar;

		protected override float NaturalWindowHeight() => 2f * StandardMargin + 2f * Utilities.ShortSpacing + Utilities.TinyTextLineHeight + Utilities.StandardRowHeight * (this.cachedPawnSurfaces.Count + 1f);

		protected override int GetColonistCount()
		{
			return Find.VisibleMap.mapPawns.FreeColonistsCount;
		}

		protected override IEnumerable<PawnSurface> GetCachedSurfaces()
		{
			foreach (Pawn pawn in Find.VisibleMap.mapPawns.FreeColonists.OrderBy(p => p.CachedPawnLabel()))
			{
				yield return Controller.GetManager.GetPawnSurface(pawn);
			}
		}

		private void DrawPriorityIndicators(Rect rect)
		{
			Text.Font = GameFont.Tiny;
			GUI.color = Utilities.IndicatorsColour;

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<= " + "HigherPriority".CachedTranslation());

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "LowerPriority".CachedTranslation() + " =>");

			//Reset
			Text.Font = GameFont.Small;
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
		}

		public override void WindowUpdate()
		{
			//Check for invalid drag
			if (Controller.CurrentDraggable != null && !Input.GetMouseButton(0) && Event.current.type != EventType.MouseUp)
			{
				Controller.CurrentDraggable.OnDrop();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			//Build rects
			Rect indicatorsRect = new Rect(inRect.xMax - Utilities.PawnSurfaceWidth - Utilities.SpaceForScrollBar + Utilities.ShortSpacing, inRect.y, Utilities.PawnSurfaceWidth - 3f * Utilities.ShortSpacing, Utilities.TinyTextLineHeight);

			float topControlsY = indicatorsRect.yMax + Utilities.ShortSpacing;

			Rect compareSkillsButtonRect = new Rect(inRect.x - this.horizontalOffset + Utilities.ShortSpacing, topControlsY, Utilities.SpaceForPawnLabel, Utilities.DraggableDiameter);
			Rect primaryWorkButtonRect = new Rect(compareSkillsButtonRect.xMax + 2f * Utilities.ShortSpacing, topControlsY, Utilities.DraggableDiameter, Utilities.DraggableDiameter);
			Rect primariesRect = new Rect(primaryWorkButtonRect.xMax, topControlsY, Utilities.PawnSurfaceWidth, Utilities.DraggableDiameter);

			Rect scrollViewBox = new Rect(inRect.x, indicatorsRect.yMax + Utilities.StandardRowHeight, inRect.width, inRect.height - indicatorsRect.height - Utilities.StandardRowHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButton + Utilities.PawnSurfaceWidth, this.cachedPawnSurfaces.Count * Utilities.StandardRowHeight);

			if (scrollViewInnerRect.width > scrollViewOuterRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw indicators, primaries and list outline
			if (Event.current.type == EventType.Repaint)
			{
				this.DrawPriorityIndicators(indicatorsRect);

				Controller.GetPrimaries.DrawSurface(primariesRect);

				Utilities.BoxOutline(scrollViewBox);
			}

			//Compare Skills button
			if (Widgets.ButtonText(compareSkillsButtonRect, "DD_WorkTab_Work_CompareSkills".CachedTranslation(), true, false, true))
			{
				Find.WindowStack.Add(new Window_ColonistSkills());

				Utilities.UserFeedbackChain(WorkSound.CompareSkillsMapChanged);
			}

			//Float menu button
			Utilities.WorkButton(primaryWorkButtonRect, null);

			//Check for primary-related GUI triggers
			Controller.GetPrimaries.DoWorkTabEventChecks(primariesRect);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			Controller.CurrentDraggable?.OnDrag(); //Update draggable position within the list

			//Determine which surfaces will actually be seen
			DingoUtils.VisibleScrollviewIndexes(this.scrollPosition.y, scrollViewOuterRect.height, Utilities.StandardRowHeight, this.cachedPawnSurfaces.Count, out int FirstRenderedIndex, out int LastRenderedIndex);

			float dynamicVerticalY = scrollViewInnerRect.yMin + FirstRenderedIndex * Utilities.StandardRowHeight; //The .y value of the first rendered surface

			for (int i = FirstRenderedIndex; i < LastRenderedIndex; i++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[i];

				Rect labelRect = new Rect(scrollViewInnerRect.x, dynamicVerticalY, Utilities.SpaceForPawnLabel, Utilities.StandardRowHeight);
				Rect buttonRect = new Rect(labelRect.xMax + 2f * Utilities.ShortSpacing, dynamicVerticalY + Utilities.ShortSpacing, Utilities.DraggableDiameter, Utilities.DraggableDiameter);
				Rect surfaceRect = new Rect(buttonRect.xMax, dynamicVerticalY, Utilities.PawnSurfaceWidth, Utilities.StandardRowHeight);

				if (Event.current.type == EventType.Repaint)
				{
					if (i != 0)
					{
						Utilities.ListSeparator(scrollViewInnerRect, dynamicVerticalY);
					}

					surface.DrawSurface(surfaceRect);
				}

				Utilities.PawnLabel(labelRect, surface.pawn);

				Utilities.WorkButton(buttonRect, surface);

				surface.DoWorkTabEventChecks(surfaceRect);

				dynamicVerticalY += Utilities.StandardRowHeight;
			}

			//Render current Draggable on top of other textures
			DraggableWork currentDraggable = Controller.CurrentDraggable;

			if (Event.current.type == EventType.Repaint && currentDraggable != null)
			{
				Rect drawRect = currentDraggable.position.ToWorkRect();

				currentDraggable.DrawTexture(drawRect);
			}

			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();

			//Make sure drag gets reset
			Controller.CurrentDraggable?.OnDrop();
			Controller.CurrentDraggable = null;

			Controller.CopyPrioritiesReference = null;
		}

		public Window_WorkTab() : base()
		{
		}
	}
}
