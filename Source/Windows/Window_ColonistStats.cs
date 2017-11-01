using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab
{
	public class Window_ColonistStats : MainTabWindow_DD
	{
		protected override float NaturalWindowWidth() => Utilities.SpaceForPawnLabel + this.statsSurfaceWidth + 2f * Utilities.ShortSpacing + 2f * StandardMargin + Utilities.SpaceForScrollBar;

		protected override float NaturalWindowHeight() => statsRowHeight * (this.cachedColonistCount + 1f) + 2f * Utilities.ShortSpacing + 2f * StandardMargin;

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

		private void DoSortingChecks(EventData data, WorkTypeDef sortDef)
		{
			if (data.type == EventType.MouseDown)
			{
				if (data.button == 0)
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
				}

				if (data.button == 1)
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
				}

				if (Controller.UseSounds)
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

			//Cache current event details
			Event currentEvent = Event.current;
			EventData data;
			List<PrimaryWork> primariesList = Controller.GetPrimaries.PrimaryWorkList;

			if (currentEvent.isMouse)
			{
				data = new EventData(currentEvent.type, currentEvent.mousePosition, currentEvent.button, currentEvent.shift);
			}

			else
			{
				data = new EventData(currentEvent.type, currentEvent.mousePosition, -1, false);
			}

			//Build rects
			Rect toggleMapRect = new Rect(inRect.x - this.horizontalOffset, inRect.y, Utilities.ShortSpacing + Utilities.SpaceForPawnLabel, statsRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, inRect.y, inRect.width - toggleMapRect.width, statsRowHeight);

			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - statsRowHeight);
			Rect scrollViewOutRect = scrollViewBox.ContractedBy(Utilities.ShortSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, Utilities.SpaceForPawnLabel + this.statsSurfaceWidth, this.cachedColonistCount * statsRowHeight);

			if (scrollViewInnerRect.width > scrollViewOutRect.width)
			{
				scrollViewInnerRect.yMax += Utilities.SpaceForScrollBar;
			}

			//Draw list outline
			if (data.type == EventType.Repaint)
			{
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

				if (primeRect.Contains(data.mousePosition))
				{
					Widgets.DrawHighlight(primeRect);

					TooltipHandler.TipRegion(primeRect, Utilities.DraggableTooltip(primaryDef, true, true, false, false, null));

					this.DoSortingChecks(data, primaryDef);
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

			Widgets.BeginScrollView(scrollViewOutRect, ref this.scrollPosition, scrollViewInnerRect, true);

			data.mousePosition = Event.current.mousePosition;

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			for (int k = 0; k < this.cachedPawnSurfaces.Count; k++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[k];

				Pawn pawn = surface.pawn;

				if (firstPawnDrawn)
				{
					Utilities.ListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, Utilities.SpaceForPawnLabel, statsRowHeight);
				Rect surfaceRect = new Rect(nameRect.xMax, currentVerticalPosition, this.statsSurfaceWidth, statsRowHeight);
				float surfaceRectCenterY = surfaceRect.center.y;

				Utilities.PawnLabel(nameRect, pawn, data);

				for (int p = 0; p < primariesList.Count; p++)
				{
					WorkTypeDef def = primariesList[p].def;
					DraggableWork matchingDraggable = surface.childByDef[def];
					Rect drawRect = new Vector2(this.primariesPositions[def], surfaceRectCenterY).ToDraggableRect(smallDraggableDiameter);

					matchingDraggable.DrawTexture(drawRect);

					if (drawRect.Contains(data.mousePosition))
					{
						string tipString = Utilities.DraggableTooltip(def, false, true, matchingDraggable.CompletelyDisabled, matchingDraggable.Disabled, pawn);

						TooltipHandler.TipRegion(drawRect, tipString);
					}
				}

				currentVerticalPosition += statsRowHeight;
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
