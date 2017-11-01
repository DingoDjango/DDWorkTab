using System.Collections.Generic;
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
			EventType currentEventType = Event.current.type;

			if (Controller.CurrentDraggable != null && !Input.GetMouseButton(0) && currentEventType != EventType.MouseUp)
			{
				Controller.CurrentDraggable.OnDrop();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			//Cache current event details
			Event currentEvent = Event.current;
			EventData data;

			if (currentEvent.isMouse)
			{
				data = new EventData(currentEvent.type, currentEvent.mousePosition, currentEvent.button, currentEvent.shift);
			}

			else
			{
				data = new EventData(currentEvent.type, currentEvent.mousePosition, -1, false);
			}

			//Build rects
			Rect indicatorsRect = new Rect(inRect.xMax - Utilities.PawnSurfaceWidth - Utilities.SpaceForScrollBar, inRect.y, Utilities.PawnSurfaceWidth - Utilities.ShortSpacing, Utilities.TinyTextLineHeight);

			float topControlsY = indicatorsRect.yMax + Utilities.ShortSpacing;

			Rect compareSkillsButtonRect = new Rect(inRect.x - this.horizontalOffset + Utilities.ShortSpacing, topControlsY, Utilities.SpaceForPawnLabel, Utilities.DraggableTextureDiameter);
			Rect disableAllWorkButtonRect = new Rect(compareSkillsButtonRect.xMax + 2f * Utilities.ShortSpacing, topControlsY, Utilities.DraggableTextureDiameter, Utilities.DraggableTextureDiameter);
			Rect resetAllWorkButtonRect = new Rect(disableAllWorkButtonRect.xMax + Utilities.ShortSpacing, topControlsY, Utilities.DraggableTextureDiameter, Utilities.DraggableTextureDiameter);
			Rect primariesRect = new Rect(compareSkillsButtonRect.xMax + Utilities.SpaceForWorkButtons, topControlsY, Utilities.PawnSurfaceWidth, Utilities.DraggableTextureDiameter);

			Rect scrollViewBox = new Rect(inRect.x, indicatorsRect.yMax + Utilities.StandardRowHeight, inRect.width, inRect.height - indicatorsRect.height - Utilities.StandardRowHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + Utilities.SpaceForWorkButtons + Utilities.PawnSurfaceWidth, this.cachedColonistCount * Utilities.StandardRowHeight);

			if (scrollViewInnerRect.width > scrollViewOuterRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw indicators, primaries and list outline
			if (data.type == EventType.Repaint)
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

			//Disable Work (All Map Colonists) button
			Utilities.Button(disableAllWorkButtonRect, ButtonType.DisableAllWork, data, null);

			//Reset Work (All Map Colonists) button
			Utilities.Button(resetAllWorkButtonRect, ButtonType.ResetAllWork, data, null);

			//Check for primary-related GUI triggers
			Controller.GetPrimaries.DoEventChecks(primariesRect, data);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			data.mousePosition = currentEvent.mousePosition; //Mouse position inside the list (inRect position + scrollPosition)

			Controller.CurrentDraggable?.OnDrag(data);

			float currentVerticalPosition = scrollViewInnerRect.yMin;

			for (int i = 0; i < this.cachedPawnSurfaces.Count; i++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[i];

				if (data.type == EventType.Repaint && i != 0)
				{
					Utilities.ListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				Rect pawnLabelRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, Utilities.SpaceForPawnLabel, Utilities.StandardRowHeight);
				Rect disableWorkRect = new Rect(pawnLabelRect.xMax + 2f * Utilities.ShortSpacing, currentVerticalPosition + Utilities.ShortSpacing, Utilities.DraggableTextureDiameter, Utilities.DraggableTextureDiameter);
				Rect resetWorkRect = new Rect(disableWorkRect.xMax + Utilities.ShortSpacing, disableWorkRect.y, disableWorkRect.width, disableWorkRect.height);
				Rect surfaceRect = new Rect(pawnLabelRect.xMax + Utilities.SpaceForWorkButtons, currentVerticalPosition, Utilities.PawnSurfaceWidth, Utilities.StandardRowHeight);

				Utilities.PawnLabel(pawnLabelRect, surface.pawn, data);

				Utilities.Button(disableWorkRect, ButtonType.DisableWork, data, surface);

				Utilities.Button(resetWorkRect, ButtonType.ResetWork, data, surface);

				if (data.type == EventType.Repaint)
				{
					surface.DrawSurface(surfaceRect);
				}

				surface.DoEventChecks(surfaceRect, data);

				currentVerticalPosition += Utilities.StandardRowHeight;
			}

			DraggableWork currentDraggable = Controller.CurrentDraggable;

			//Render current Draggable on top of other textures
			if (data.type == EventType.Repaint && currentDraggable != null)
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
		}

		public void OnPrimaryShiftClick(int clickInt, WorkTypeDef def)
		{
			if (clickInt != 0)
			{
				for (int i = 0; i < this.cachedPawnSurfaces.Count; i++)
				{
					this.cachedPawnSurfaces[i].OnPrimaryShiftClick(clickInt, def);
				}
			}

			if (Controller.VerboseMessages)
			{
				string shiftCompletedText = "DD_WorkTab_Message_PrimaryShiftClick".CachedTranslation(new string[] { def.labelShort });

				Messages.Message(shiftCompletedText, MessageTypeDefOf.SilentInput);
			}

			if (Controller.UseSounds)
			{
				Utilities.TaskCompleted.PlayOneShotOnCamera(null);
			}
		}

		public Window_WorkTab() : base()
		{
		}
	}
}
