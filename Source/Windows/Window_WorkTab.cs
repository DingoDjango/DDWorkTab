using System.Collections.Generic;
using System.Linq;
using DD_WorkTab.Base;
using DD_WorkTab.Draggables;
using DD_WorkTab.Tools;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Windows
{
	public class Window_WorkTab : MainTabWindow_DD
	{
		protected override float NaturalWindowWidth() => 2f * StandardMargin + 2f * Utilities.ShortSpacing + Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButtons + Utilities.PawnSurfaceWidth + Utilities.SpaceForScrollBar;

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
			Text.Anchor = TextAnchor.UpperLeft;
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
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
			Rect indicatorsRect = new Rect(inRect.xMax - Utilities.PawnSurfaceWidth - Utilities.SpaceForScrollBar, inRect.y, Utilities.PawnSurfaceWidth - Utilities.ShortSpacing, Utilities.TinyTextLineHeight);

			float topControlsY = indicatorsRect.yMax + Utilities.ShortSpacing;

			Rect compareSkillsButtonRect = new Rect(inRect.x - this.horizontalOffset + Utilities.ShortSpacing, topControlsY, Utilities.SpaceForPawnLabel, Utilities.DraggableDiameter);
			Rect primaryWorkButtonRect = new Rect(compareSkillsButtonRect.xMax + 2f * Utilities.ShortSpacing, topControlsY, Utilities.DraggableDiameter, Utilities.DraggableDiameter);
			Rect primariesRect = new Rect(primaryWorkButtonRect.xMax, topControlsY, Utilities.PawnSurfaceWidth, Utilities.DraggableDiameter);

			Rect scrollViewBox = new Rect(inRect.x, indicatorsRect.yMax + Utilities.StandardRowHeight, inRect.width, inRect.height - indicatorsRect.height - Utilities.StandardRowHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButtons + Utilities.PawnSurfaceWidth, this.cachedPawnSurfaces.Count * Utilities.StandardRowHeight);

			if (scrollViewInnerRect.width > scrollViewOuterRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw indicators, primaries and list outline
			if (Event.current.type == EventType.Repaint)
			{
				this.DrawPriorityIndicators(indicatorsRect);

				Controller.GetPrimaries.DrawPrimaryDraggables(primariesRect);

				Utilities.BoxOutline(scrollViewBox);
			}

			//Compare Skills button
			Text.Font = GameFont.Small;

			if (Widgets.ButtonText(compareSkillsButtonRect, "DD_WorkTab_Work_CompareSkills".CachedTranslation(), true, false, true))
			{
				Find.WindowStack.Add(new Window_ColonistSkills());
			}

			//Float menu button
			Utilities.WorkButton(primaryWorkButtonRect, null);

			//Check for primary-related GUI triggers
			Controller.GetPrimaries.DoEventChecks(primariesRect);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			Controller.CurrentDraggable?.OnDrag(); //Update draggable position within the list

			//Determine which surfaces will actually be seen
			Utilities.ExtraUtilities.VisibleScrollviewIndexes(this.scrollPosition.y, scrollViewOuterRect.height, Utilities.StandardRowHeight, this.cachedPawnSurfaces.Count, out int FirstIndex, out int LastIndex);

			float dynamicVerticalY = scrollViewInnerRect.yMin + FirstIndex * Utilities.StandardRowHeight; //The .y value of the first rendered surface

			for (int i = FirstIndex; i < LastIndex; i++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[i];
				Pawn pawn = surface.pawn;

				Rect pawnLabelRect = new Rect(scrollViewInnerRect.x, dynamicVerticalY, Utilities.SpaceForPawnLabel, Utilities.StandardRowHeight);
				Rect workButtonRect = new Rect(pawnLabelRect.xMax + 2f * Utilities.ShortSpacing, dynamicVerticalY + Utilities.ShortSpacing, Utilities.DraggableDiameter, Utilities.DraggableDiameter);
				Rect surfaceRect = new Rect(workButtonRect.xMax, dynamicVerticalY, Utilities.PawnSurfaceWidth, Utilities.StandardRowHeight);

				if (Event.current.type == EventType.Repaint)
				{
					if (i != 0)
					{
						Utilities.ListSeparator(scrollViewInnerRect, dynamicVerticalY);
					}

					surface.DrawSurface(surfaceRect);
				}

				Utilities.PawnLabel(pawnLabelRect, pawn);

				Utilities.WorkButton(workButtonRect, surface);

				surface.DoEventChecks(surfaceRect);

				dynamicVerticalY += Utilities.StandardRowHeight;
			}

			//Render current Draggable on top of other textures
			DraggableWork currentDraggable = Controller.CurrentDraggable;

			if (Event.current.type == EventType.Repaint && currentDraggable != null)
			{
				Rect dragRect = currentDraggable.position.ToDraggableRect();

				currentDraggable.DrawTexture(dragRect);
			}

			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();

			//Make sure drag gets reset
			if (Controller.CurrentDraggable != null)
			{
				Controller.CurrentDraggable.OnDrop();
				Controller.CurrentDraggable = null;
			}

			Controller.CopyPrioritiesReference = null;
		}

		public Window_WorkTab() : base()
		{
		}
	}
}
