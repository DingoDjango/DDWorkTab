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

		private WorkTypeDef sortingDef;

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
			this.windowRect = new Rect(((float)UI.screenWidth - this.InitialSize.x) / 2f, ((float)UI.screenHeight - this.InitialSize.y) / 2f, this.InitialSize.x, this.InitialSize.y);
			this.windowRect = this.windowRect.Rounded();
		}

		private void DoSortingChecks(WorkTypeDef sortDef)
		{
			if (Event.current.type == EventType.MouseDown)
			{
				bool changedState = false;

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

					changedState = true;
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

					changedState = true;
				}

				if (changedState)
				{
					WorkSound sound = this.sortingOrder != SortOrder.Undefined ? WorkSound.SortedSkills : WorkSound.UnsortedSkills;

					Utilities.UserFeedbackChain(sound);

					this.mustRecacheColonists = true;
				}

				Event.current.Use();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.SetSizeAndPosition();

			List<PrimaryWork> primariesList = Controller.GetPrimaries.PrimaryWorkList;

			//Build rects
			Rect toggleMapRect = new Rect(inRect.x - this.horizontalOffset, inRect.y + Utilities.TinyTextLineHeight, Utilities.ShortSpacing + Utilities.SpaceForPawnLabel, Utilities.SmallRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, toggleMapRect.y, inRect.width - toggleMapRect.width, Utilities.SmallRowHeight);

			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - Utilities.SmallRowHeight - Utilities.TinyTextLineHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + Utilities.SmallSurfaceWidth, this.cachedPawnSurfaces.Count * Utilities.SmallRowHeight);

			if (scrollViewInnerRect.width > scrollViewOuterRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw window title and list outline
			if (Event.current.type == EventType.Repaint)
			{
				GUI.color = Utilities.Orange;
				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Tiny;

				Widgets.Label(inRect, "DD_WorkTab_ColonistSkills_Title".CachedTranslation());

				Text.Font = GameFont.Small; //Reset
				Text.Anchor = TextAnchor.UpperLeft; //Reset

				Utilities.BoxOutline(scrollViewBox);
			}

			//Toggle Displayed Pawns button
			Text.Font = GameFont.Small;

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(Utilities.ShortSpacing), this.ToggleButtonText, true, false, true))
			{
				Controller.ColonistSkillsVisibleMap = !Controller.ColonistSkillsVisibleMap;

				this.mustRecacheColonists = true;

				Utilities.UserFeedbackChain(WorkSound.CompareSkillsMapChanged);
			}

			//Draw primaries
			Vector2 primaryPositions = new Vector2(primaryTypesRect.x + Utilities.ShortSpacing + (Utilities.SmallDraggableDiameter / 2f), primaryTypesRect.center.y);

			for (int j = 0; j < primariesList.Count; j++)
			{
				PrimaryWork primary = primariesList[j];
				WorkTypeDef primaryDef = primary.def;

				this.primariesPositions[primaryDef] = primaryPositions.x;

				Rect primeRect = primaryPositions.ToDraggableRect(Utilities.SmallDraggableDiameter);

				primary.DrawTexture(primeRect);

				if (primeRect.Contains(Event.current.mousePosition))
				{
					Widgets.DrawHighlight(primeRect);

					TooltipHandler.TipRegion(primeRect, Utilities.DraggableTooltip(primaryDef, true, true, false, false, null));

					this.DoSortingChecks(primaryDef);
				}

				//Draw little arrow indicator below work type
				if (this.sortingOrder != SortOrder.Undefined && this.sortingDef == primaryDef)
				{
					Texture2D icon = this.sortingOrder == SortOrder.Descending ? Utilities.SortingDescendingIcon : Utilities.SortingAscendingIcon;
					Rect iconRect = new Rect(primeRect.xMax - (float)icon.width, primeRect.yMax + 1f, icon.width, icon.height);

					GUI.DrawTexture(iconRect, icon);

					Widgets.DrawHighlight(new Rect(primeRect.xMin - 3f, primeRect.yMin - 3f, primeRect.width + 6f, primeRect.height + 6f));
				}

				primaryPositions.x += Utilities.ShortSpacing + Utilities.SmallDraggableDiameter;
			}

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//Determine which surfaces will actually be seen
			Utilities.ExtraUtilities.VisibleScrollviewIndexes(this.scrollPosition.y, scrollViewOuterRect.height, Utilities.SmallRowHeight, this.cachedPawnSurfaces.Count, out int FirstIndex, out int LastIndex);

			float dynamicVerticalY = scrollViewInnerRect.yMin + FirstIndex * Utilities.SmallRowHeight; //The .y value of the first rendered surface

			for (int k = FirstIndex; k < LastIndex; k++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[k];

				Pawn pawn = surface.pawn;

				if (k != 0)
				{
					Utilities.ListSeparator(scrollViewInnerRect, dynamicVerticalY);
				}

				Rect nameRect = new Rect(scrollViewInnerRect.x, dynamicVerticalY, Utilities.SpaceForPawnLabel, Utilities.SmallRowHeight);
				Rect surfaceRect = new Rect(nameRect.xMax, dynamicVerticalY, Utilities.SmallSurfaceWidth, Utilities.SmallRowHeight);
				float surfaceRectCenterY = surfaceRect.center.y;

				Utilities.PawnLabel(nameRect, pawn);

				for (int p = 0; p < primariesList.Count; p++)
				{
					WorkTypeDef def = primariesList[p].def;
					DraggableWork matchingDraggable = surface.childByDef[def];
					Rect drawRect = new Vector2(this.primariesPositions[def], surfaceRectCenterY).ToDraggableRect(Utilities.SmallDraggableDiameter);

					matchingDraggable.DrawTexture(drawRect);

					if (drawRect.Contains(Event.current.mousePosition))
					{
						string tipString = Utilities.DraggableTooltip(def, false, true, matchingDraggable.CompletelyDisabled, matchingDraggable.Disabled, pawn);

						Widgets.DrawHighlight(drawRect);

						TooltipHandler.TipRegion(drawRect, tipString);
					}
				}

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
