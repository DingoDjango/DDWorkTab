using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DD_WorkTab.Base;
using DD_WorkTab.Draggables;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab.Windows
{
	public class Window_WorkTab : MainTabWindow_DD
	{
		protected override float NaturalWindowWidth() => Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButtons + Utilities.PawnSurfaceWidth + 2f * Utilities.ShortSpacing + 2f * StandardMargin + Utilities.SpaceForScrollBar;

		protected override float NaturalWindowHeight() => Utilities.TinyTextLineHeight + Utilities.StandardRowHeight * (this.cachedColonistCount + 1f) + 2f * Utilities.ShortSpacing + 2f * StandardMargin;

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

			Rect compareSkillsButtonRect = new Rect(inRect.x - this.horizontalOffset + Utilities.ShortSpacing, topControlsY, Utilities.SpaceForPawnLabel, Utilities.DraggableTextureDiameter);
			Rect primaryWorkButtonRect = new Rect(compareSkillsButtonRect.xMax + 2f * Utilities.ShortSpacing, topControlsY, Utilities.DraggableTextureDiameter, Utilities.DraggableTextureDiameter);
			Rect primariesRect = new Rect(primaryWorkButtonRect.xMax + Utilities.ShortSpacing, topControlsY, Utilities.PawnSurfaceWidth, Utilities.DraggableTextureDiameter);

			Rect scrollViewBox = new Rect(inRect.x, indicatorsRect.yMax + Utilities.StandardRowHeight, inRect.width, inRect.height - indicatorsRect.height - Utilities.StandardRowHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButtons + Utilities.PawnSurfaceWidth, this.cachedColonistCount * Utilities.StandardRowHeight);

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

			if (Widgets.ButtonText(compareSkillsButtonRect, "DD_WorkTab_ButtonColonistStats".CachedTranslation(), true, false, true))
			{
				Find.WindowStack.Add(new Window_ColonistStats());
			}

			//Float menu button
			Utilities.WorkButton(primaryWorkButtonRect, null);

			//Check for primary-related GUI triggers
			Controller.GetPrimaries.DoEventChecks(primariesRect);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			Controller.CurrentDraggable?.OnDrag();

			//Determine which surfaces will actually be seen and ignore all others
			int FirstIndexToRender = (int)(this.scrollPosition.y / Utilities.StandardRowHeight); //Get the first list item that should be visible
			int TotalIndexesToRender = (int)(scrollViewOuterRect.height / Utilities.StandardRowHeight) + 2; //Account for partly rendered surfaces on top/bottom
			int LastIndexToRender = Mathf.Min(FirstIndexToRender + TotalIndexesToRender, this.cachedPawnSurfaces.Count); //Get the last item to render, don't go over list.Count
			float dynamicVerticalY = scrollViewInnerRect.yMin + FirstIndexToRender * Utilities.StandardRowHeight; //The .y value of the first rendered surface

			for (int i = FirstIndexToRender; i < LastIndexToRender; i++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[i];
				Pawn pawn = surface.pawn;

				Rect pawnLabelRect = new Rect(scrollViewInnerRect.x, dynamicVerticalY, Utilities.SpaceForPawnLabel, Utilities.StandardRowHeight);
				Rect workButtonRect = new Rect(pawnLabelRect.xMax + 2f * Utilities.ShortSpacing, dynamicVerticalY + Utilities.ShortSpacing, Utilities.DraggableTextureDiameter, Utilities.DraggableTextureDiameter);
				Rect surfaceRect = new Rect(workButtonRect.xMax + Utilities.ShortSpacing, dynamicVerticalY, Utilities.PawnSurfaceWidth, Utilities.StandardRowHeight);

				if (Event.current.type == EventType.Repaint)
				{
					if (i != 0)
					{
						Utilities.ListSeparator(scrollViewInnerRect, dynamicVerticalY);
					}

					surface.DrawSurface(surfaceRect);

					Utilities.PawnLabel(pawnLabelRect, pawn);

					if (pawnLabelRect.Contains(Event.current.mousePosition))
					{
						Widgets.DrawHighlight(pawnLabelRect);

						TooltipHandler.TipRegion(pawnLabelRect, "ClickToJumpTo".CachedTranslation() + pawn.GetTooltip().text);
					}
				}

				else
				{
					if (Event.current.type == EventType.MouseDown)
					{
						if (pawnLabelRect.Contains(Event.current.mousePosition))
						{
							CameraJumper.TryJumpAndSelect(pawn);

							this.Close(false);
							Find.WindowStack.TryRemove(typeof(Window_ColonistStats), false); //Close Skills window if open
						}
					}
				}

				Utilities.WorkButton(workButtonRect, surface);

				surface.DoEventChecks(surfaceRect);

				dynamicVerticalY += Utilities.StandardRowHeight;
			}

			DraggableWork currentDraggable = Controller.CurrentDraggable;

			//Render current Draggable on top of other textures
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

			Controller.CurrentDraggable = null;
			Controller.CopyPrioritiesReference = null;
		}		

		public Window_WorkTab() : base()
		{
		}
	}
}
