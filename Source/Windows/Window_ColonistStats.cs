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
using Verse.Sound;

namespace DD_WorkTab.Windows
{
	public class Window_ColonistStats : MainTabWindow_DD
	{
		protected override float NaturalWindowWidth() => Utilities.SpaceForPawnLabel + this.statsSurfaceWidth + 2f * Utilities.ShortSpacing + 2f * StandardMargin + Utilities.SpaceForScrollBar;

		protected override float NaturalWindowHeight() => Utilities.TinyTextLineHeight + statsRowHeight * (this.cachedColonistCount + 1f) + 2f * Utilities.ShortSpacing + 2f * StandardMargin;

		private const float smallDraggableDiameter = 32f;

		private const float statsRowHeight = 2f * Utilities.ShortSpacing + smallDraggableDiameter;

		private float statsSurfaceWidth = Utilities.ShortSpacing + Controller.GetPrimaries.PrimaryWorkList.Count * (smallDraggableDiameter + Utilities.ShortSpacing);

		private string ToggleButtonText => (Controller.ColonistStatsOnlyVisibleMap ? "DD_WorkTab_ColonistStats_ButtonVisibleMap" : "DD_WorkTab_ColonistStats_ButtonAllMaps").CachedTranslation();

		private Dictionary<WorkTypeDef, float> primariesPositions = new Dictionary<WorkTypeDef, float>();

		private WorkTypeDef sortingDef;

		private SortOrder sortingOrder = SortOrder.Undefined;

		protected override int GetColonistCount()
		{
			if (Controller.ColonistStatsOnlyVisibleMap)
			{
				return this.currentMap.mapPawns.FreeColonistsCount;
			}

			else
			{
				int count = 0;

				for (int i = 0; i < Find.Maps.Count; i++)
				{
					count += Find.Maps[i].mapPawns.FreeColonistsCount;
				}

				return count;
			}
		}

		protected override IEnumerable<PawnSurface> GetCachedSurfaces()
		{
			IEnumerable<Pawn> pawnsList = Controller.ColonistStatsOnlyVisibleMap ? this.currentMap.mapPawns.FreeColonists : PawnsFinder.AllMaps_FreeColonists;

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

				if (Controller.UseSounds && changedState)
				{
					SoundDef soundToUse = this.sortingOrder == SortOrder.Undefined ? SoundDefOf.TickLow : SoundDefOf.TickHigh;

					soundToUse.PlayOneShotOnCamera(null);
				}

				this.mustRecacheColonists = true;

				Event.current.Use();
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.SetSizeAndPosition();

			List<PrimaryWork> primariesList = Controller.GetPrimaries.PrimaryWorkList;

			//Build rects
			Rect toggleMapRect = new Rect(inRect.x - this.horizontalOffset, inRect.y + Utilities.TinyTextLineHeight, Utilities.ShortSpacing + Utilities.SpaceForPawnLabel, statsRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, toggleMapRect.y, inRect.width - toggleMapRect.width, statsRowHeight);

			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - statsRowHeight - Utilities.TinyTextLineHeight);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, Utilities.SpaceForPawnLabel + this.statsSurfaceWidth, this.cachedColonistCount * statsRowHeight);

			if (scrollViewInnerRect.width > scrollViewOuterRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw list outline
			if (Event.current.type == EventType.Repaint)
			{
				GUI.color = Utilities.Orange;
				Text.Anchor = TextAnchor.UpperCenter;
				Text.Font = GameFont.Tiny;

				Widgets.Label(inRect, "Compare Colonist Skills");

				Text.Font = GameFont.Small; //Reset
				Text.Anchor = TextAnchor.UpperLeft; //Reset

				Utilities.BoxOutline(scrollViewBox);
			}

			//Toggle Displayed Pawns button
			Text.Font = GameFont.Small;

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(Utilities.ShortSpacing), this.ToggleButtonText, true, false, true))
			{
				Controller.ColonistStatsOnlyVisibleMap = !Controller.ColonistStatsOnlyVisibleMap;

				this.mustRecacheColonists = true;

				if (Controller.UseSounds)
				{
					SoundDefOf.DialogBoxAppear.PlayOneShotOnCamera(null);
				}
			}

			//Draw primaries
			Vector2 primaryPositions = new Vector2(primaryTypesRect.x + Utilities.ShortSpacing + (smallDraggableDiameter / 2f), primaryTypesRect.center.y);

			for (int j = 0; j < primariesList.Count; j++)
			{
				PrimaryWork primary = primariesList[j];
				WorkTypeDef primaryDef = primary.def;

				this.primariesPositions[primaryDef] = primaryPositions.x;

				Rect primeRect = primaryPositions.ToDraggableRect(smallDraggableDiameter);

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

				primaryPositions.x += Utilities.ShortSpacing + smallDraggableDiameter;
			}

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//Determine which surfaces will actually be seen and ignore all others
			int FirstIndexToRender = (int)(this.scrollPosition.y / statsRowHeight); //Get the first list item that should be visible
			int TotalIndexesToRender = (int)(scrollViewOuterRect.height / statsRowHeight) + 2; //Account for partly rendered surfaces on top/bottom
			int LastIndexToRender = Mathf.Min(FirstIndexToRender + TotalIndexesToRender, this.cachedPawnSurfaces.Count); //Get the last item to render, don't go over list.Count
			float dynamicVerticalY = scrollViewInnerRect.yMin + FirstIndexToRender * statsRowHeight; //The .y value of the first rendered surface

			for (int k = FirstIndexToRender; k < LastIndexToRender; k++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[k];

				Pawn pawn = surface.pawn;

				if (k != 0)
				{
					Utilities.ListSeparator(scrollViewInnerRect, dynamicVerticalY);
				}

				Rect nameRect = new Rect(scrollViewInnerRect.x, dynamicVerticalY, Utilities.SpaceForPawnLabel, statsRowHeight);
				Rect surfaceRect = new Rect(nameRect.xMax, dynamicVerticalY, this.statsSurfaceWidth, statsRowHeight);
				float surfaceRectCenterY = surfaceRect.center.y;

				Utilities.PawnLabel(nameRect, pawn);

				for (int p = 0; p < primariesList.Count; p++)
				{
					WorkTypeDef def = primariesList[p].def;
					DraggableWork matchingDraggable = surface.childByDef[def];
					Rect drawRect = new Vector2(this.primariesPositions[def], surfaceRectCenterY).ToDraggableRect(smallDraggableDiameter);

					matchingDraggable.DrawTexture(drawRect);

					if (drawRect.Contains(Event.current.mousePosition))
					{
						string tipString = Utilities.DraggableTooltip(def, false, true, matchingDraggable.CompletelyDisabled, matchingDraggable.Disabled, pawn);

						Widgets.DrawHighlight(drawRect);

						TooltipHandler.TipRegion(drawRect, tipString);
					}
				}

				dynamicVerticalY += statsRowHeight;
			}

			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();

			this.sortingDef = null;
			this.sortingOrder = SortOrder.Undefined;
		}

		public Window_ColonistStats() : base()
		{
			this.closeOnClickedOutside = true;
		}
	}
}
