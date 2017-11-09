using System.Collections.Generic;
using System.Linq;
using DD_WorkTab.Base;
using DD_WorkTab.Draggables;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Primaries;
using DD_WorkTab.Tools;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Windows
{
	public class Window_ColonistSkills : MainTabWindow_DD
	{
		protected override float NaturalWindowWidth() => 2f * StandardMargin + 2f * Utilities.ShortSpacing + Utilities.SpaceForPawnLabel + Utilities.SmallSurfaceWidth + Utilities.SpaceForScrollBar;

		protected override float NaturalWindowHeight() => 2f * StandardMargin + 2f * Utilities.ShortSpacing + Utilities.TinyTextLineHeight + Utilities.SmallRowHeight * (this.cachedPawnSurfaces.Count + 1f);

		private string ToggleButtonText => (Controller.ColonistSkillsVisibleMap ? "DD_WorkTab_ColonistSkills_VisibleMap" : "DD_WorkTab_ColonistSkills_AllMaps").CachedTranslation();

		private Dictionary<WorkTypeDef, float> primariesPositions = new Dictionary<WorkTypeDef, float>();

		private WorkTypeDef sortingDef = null;

		private SortOrder sortingOrder = SortOrder.Undefined;

		protected override int GetColonistCount()
		{
			if (Controller.ColonistSkillsVisibleMap)
			{
				return Find.VisibleMap.mapPawns.FreeColonistsCount;
			}

			else
			{
				return PawnsFinder.AllMaps_FreeColonists.Count();
			}
		}

		protected override IEnumerable<PawnSurface> GetCachedSurfaces()
		{
			IEnumerable<Pawn> pawnsList = Controller.ColonistSkillsVisibleMap ? Find.VisibleMap.mapPawns.FreeColonists : PawnsFinder.AllMaps_FreeColonists;

			if (this.sortingOrder == SortOrder.Undefined || this.sortingDef == null)
			{
				pawnsList = pawnsList.OrderBy(p => p.CachedPawnLabel());
			}

			else
			{
				pawnsList = pawnsList.OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef)).ThenByDescending(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).ThenBy(p => p.CachedPawnLabel());

				if (this.sortingOrder == SortOrder.Ascending)
				{
					pawnsList = pawnsList.Reverse();
				}
			}

			foreach (Pawn pawn in pawnsList)
			{
				yield return Controller.GetManager.GetPawnSurface(pawn);
			}
		}

		//Window.SetInitialSizeAndPosition
		private void SetSizeAndPosition()
		{
			this.windowRect = new Rect((UI.screenWidth - this.InitialSize.x) / 2f, (UI.screenHeight - this.InitialSize.y) / 2f, this.InitialSize.x, this.InitialSize.y);
		}

		private void DoSortingChecks(WorkTypeDef sortDef)
		{
			bool changedSorting = false;

			if (Event.current.button == 0)
			{
				if (this.sortingDef != sortDef)
				{
					this.sortingDef = sortDef;
					this.sortingOrder = SortOrder.Descending;
				}

				else
				{
					switch (this.sortingOrder)
					{
						case SortOrder.Descending:
							this.sortingOrder = SortOrder.Ascending;
							break;
						case SortOrder.Ascending:
							this.sortingOrder = SortOrder.Undefined;
							break;
						default:
							this.sortingOrder = SortOrder.Descending;
							break;
					}
				}

				changedSorting = true;
			}

			if (Event.current.button == 1)
			{
				if (this.sortingDef != sortDef)
				{
					this.sortingDef = sortDef;
					this.sortingOrder = SortOrder.Ascending;
				}

				else
				{
					switch (this.sortingOrder)
					{
						case SortOrder.Descending:
							this.sortingOrder = SortOrder.Undefined;
							break;
						case SortOrder.Ascending:
							this.sortingOrder = SortOrder.Descending;
							break;
						default:
							this.sortingOrder = SortOrder.Ascending;
							break;
					}
				}

				changedSorting = true;
			}

			if (changedSorting)
			{
				WorkSound sound = this.sortingOrder != SortOrder.Undefined ? WorkSound.SortedSkills : WorkSound.UnsortedSkills;

				Utilities.UserFeedbackChain(sound);

				this.mustRecacheColonists = true;
			}

			Event.current.Use();
		}

		private void DrawWindowTitle(Rect rect)
		{
			GUI.color = Utilities.Orange;
			Text.Anchor = TextAnchor.UpperCenter;
			Text.Font = GameFont.Tiny;

			Widgets.Label(rect, "DD_WorkTab_ColonistSkills_Title".CachedTranslation());

			//Reset
			GUI.color = Color.white;
			Text.Anchor = TextAnchor.UpperLeft;
			Text.Font = GameFont.Small;
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.SetSizeAndPosition();

			List<PrimaryWork> primariesList = Controller.GetPrimaries.PrimaryWorkList;

			//Build rects
			Rect toggleButtonRect = new Rect(inRect.x - this.horizontalOffset, inRect.y + Utilities.TinyTextLineHeight, Utilities.ShortSpacing + Utilities.SpaceForPawnLabel, Utilities.SmallRowHeight);
			Rect primariesRect = new Rect(toggleButtonRect.xMax, toggleButtonRect.y, inRect.width - toggleButtonRect.width, Utilities.SmallRowHeight);

			Rect scrollViewBox = new Rect(inRect.x, primariesRect.yMax, inRect.width, inRect.height - Utilities.SmallRowHeight - Utilities.TinyTextLineHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + Utilities.SmallSurfaceWidth, this.cachedPawnSurfaces.Count * Utilities.SmallRowHeight);

			if (scrollViewInnerRect.width > scrollViewOuterRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw window title and list outline
			if (Event.current.type == EventType.Repaint)
			{
				this.DrawWindowTitle(inRect);

				Utilities.BoxOutline(scrollViewBox);
			}

			//Toggle Displayed Pawns button
			if (Widgets.ButtonText(toggleButtonRect.ContractedBy(Utilities.ShortSpacing), this.ToggleButtonText, true, false, true))
			{
				Controller.ColonistSkillsVisibleMap = !Controller.ColonistSkillsVisibleMap;

				this.mustRecacheColonists = true;

				Utilities.UserFeedbackChain(WorkSound.CompareSkillsMapChanged);
			}

			//Draw primaries
			Vector2 positionSetter = new Vector2(primariesRect.x + Utilities.ShortSpacing + Utilities.SmallDraggableDiameter / 2f, primariesRect.center.y);

			for (int i = 0; i < primariesList.Count; i++)
			{
				PrimaryWork primary = primariesList[i];

				this.primariesPositions[primary.def] = positionSetter.x;

				Rect drawRect = positionSetter.ToWorkRect(Utilities.SmallDraggableDiameter);

				if (Event.current.type == EventType.Repaint)
				{
					primary.DrawTexture(drawRect);

					if (drawRect.Contains(Event.current.mousePosition))
					{
						primary.OnHover(drawRect, true);
					}

					//Draw little arrow indicator below work type
					if (this.sortingOrder != SortOrder.Undefined && this.sortingDef == primary.def)
					{
						Texture2D icon = this.sortingOrder == SortOrder.Descending ? Utilities.SortingDescendingIcon : Utilities.SortingAscendingIcon;
						Rect iconRect = new Rect(drawRect.xMax - icon.width, drawRect.yMax + 1f, icon.width, icon.height);
						Rect highlightRect = new Rect(drawRect.xMin - 3f, drawRect.yMin - 3f, drawRect.width + 6f, drawRect.height + 6f);

						GUI.DrawTexture(iconRect, icon);

						Widgets.DrawHighlight(highlightRect);
					}
				}

				else if (Event.current.type == EventType.MouseDown && drawRect.Contains(Event.current.mousePosition))
				{
					this.DoSortingChecks(primary.def);
				}

				positionSetter.x += Utilities.SmallDraggableDiameter + Utilities.ShortSpacing;
			}

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//Determine which surfaces will actually be seen
			DingoUtils.VisibleScrollviewIndexes(this.scrollPosition.y, scrollViewOuterRect.height, Utilities.SmallRowHeight, this.cachedPawnSurfaces.Count, out int FirstRenderedIndex, out int LastRenderedIndex);

			float dynamicVerticalY = scrollViewInnerRect.yMin + FirstRenderedIndex * Utilities.SmallRowHeight; //The .y value of the first rendered surface

			for (int j = FirstRenderedIndex; j < LastRenderedIndex; j++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[j];

				Rect nameRect = new Rect(scrollViewInnerRect.x, dynamicVerticalY, Utilities.SpaceForPawnLabel, Utilities.SmallRowHeight);
				Rect surfaceRect = new Rect(nameRect.xMax, dynamicVerticalY, Utilities.SmallSurfaceWidth, Utilities.SmallRowHeight);
				float surfaceRectCenterY = surfaceRect.center.y;

				if (Event.current.type == EventType.Repaint)
				{
					if (j != 0)
					{
						Utilities.ListSeparator(scrollViewInnerRect, dynamicVerticalY);
					}

					//Draw surface
					for (int p = 0; p < primariesList.Count; p++)
					{
						WorkTypeDef def = primariesList[p].def;
						DraggableWork draggable = surface.childByDef[def];
						Vector2 draggablePosition = new Vector2(this.primariesPositions[def], surfaceRectCenterY);
						Rect drawRect = draggablePosition.ToWorkRect(Utilities.SmallDraggableDiameter);

						draggable.DrawTexture(drawRect);

						if (drawRect.Contains(Event.current.mousePosition))
						{
							draggable.OnHover(drawRect, true);
						}
					}
				}

				Utilities.PawnLabel(nameRect, surface.pawn);

				dynamicVerticalY += Utilities.SmallRowHeight;
			}

			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();

			this.sortingDef = null;
			this.sortingOrder = SortOrder.Undefined;
		}

		public Window_ColonistSkills() : base()
		{
			this.closeOnClickedOutside = true;
		}
	}
}
