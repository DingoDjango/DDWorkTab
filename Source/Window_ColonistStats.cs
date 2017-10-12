using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab
{
	public class Window_ColonistStats : DD_Window
	{
		private string ToggleButtonText => (DD_Settings.ColonistStatsOnlyVisibleMap ? "DD_WorkTab_ColonistStats_ToggleButton_VisibleMap" : "DD_WorkTab_ColonistStats_ToggleButton_AllMaps").CachedTranslation();

		private Dictionary<WorkTypeDef, float> primePositions = new Dictionary<WorkTypeDef, float>();

		private WorkTypeDef sortingDef;

		private SortOrder sortingOrder = SortOrder.Undefined;

		public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;

		protected override float NaturalWindowWidth()
		{
			return spaceForPawnLabel + pawnSurfaceWidth + 2 * standardSpacing + 2 * StandardMargin + 20f;
		}

		protected override float NaturalWindowHeight()
		{
			return standardRowHeight * (1f + this.cachedColonistCount) + 2f * standardSpacing + 2 * StandardMargin + 20f;
		}

		protected override int GetColonistCount()
		{
			int count = 0;

			if (DD_Settings.ColonistStatsOnlyVisibleMap)
			{
				count = this.currentMap.mapPawns.FreeColonistsCount;
			}

			else
			{
				for (int i = 0; i < Find.Maps.Count; i++)
				{
					count += Find.Maps[i].mapPawns.FreeColonistsCount;
				}
			}

			if (count == 0)
			{
				return 1;
			}

			return count;
		}

		protected override IEnumerable<PawnSurface> GetCachedSurfaces()
		{
			IEnumerable<Pawn> pawnsList = DD_Settings.ColonistStatsOnlyVisibleMap ? this.currentMap.mapPawns.FreeColonists : PawnsFinder.AllMaps_FreeColonists;

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
				yield return DragManager.GetPawnSurface(pawn);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
#if DEBUG
			Stopwatch msProfile2 = Stopwatch.StartNew();
#endif

			base.DoWindowContents(inRect);

			Rect toggleMapRect = new Rect(inRect.x - this.horizontalOffset, inRect.y, standardSpacing + spaceForPawnLabel, standardRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, inRect.y, inRect.width - toggleMapRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - primaryTypesRect.height);
			Rect scrollViewOutRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, spaceForPawnLabel + pawnSurfaceWidth, this.cachedColonistCount * standardRowHeight);

			Vector2 primePositionVector = new Vector2(primaryTypesRect.x + standardSpacing + (draggableTextureDiameter / 2f), primaryTypesRect.center.y);

			Text.Font = GameFont.Small;

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(standardSpacing), this.ToggleButtonText, true, false, true))
			{
				DD_Settings.ColonistStatsOnlyVisibleMap = !DD_Settings.ColonistStatsOnlyVisibleMap;

				this.mustRecacheColonists = true;

				if (DD_Settings.UseSounds)
				{
					SoundDefOf.DialogBoxAppear.PlayOneShotOnCamera(null);
				}
			}

			for (int j = 0; j < DD_Widgets.PrimaryDraggablesList.Count; j++)
			{
				DraggableWorkType prime = DD_Widgets.PrimaryDraggablesList[j];

				this.primePositions[prime.def] = primePositionVector.x;

				Rect primeRect = primePositionVector.ToDraggableRect();

				prime.DrawTexture(primeRect);

				if (primeRect.Contains(this.eventMousePosition))
				{
					if (Event.current.type == EventType.MouseDown)
					{
						if (Event.current.button == 0)
						{
							if (this.sortingDef != prime.def)
							{
								this.sortingDef = prime.def;
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

						if (Event.current.button == 1)
						{
							if (this.sortingDef != prime.def)
							{
								this.sortingDef = prime.def;
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

						if (DD_Settings.UseSounds)
						{
							SoundDef soundToUse = this.sortingOrder == SortOrder.Undefined ? SoundDefOf.TickLow : SoundDefOf.TickHigh;

							soundToUse.PlayOneShotOnCamera(null);
						}

						this.mustRecacheColonists = true;

						Event.current.Use();
					}

					Widgets.DrawHighlight(primeRect);

					TooltipHandler.TipRegion(primeRect, DD_Widgets.DraggableTooltip(prime.def, true, true, null));
				}

				//Draw little arrow indicator below work type
				if (this.sortingOrder != SortOrder.Undefined && this.sortingDef == prime.def)
				{
					Texture2D icon = this.sortingOrder == SortOrder.Descending ? DD_Widgets.SortingDescendingIcon : DD_Widgets.SortingAscendingIcon;
					Rect iconRect = new Rect(primeRect.xMax - (float)icon.width, primeRect.yMax + 1f, icon.width, icon.height);

					GUI.DrawTexture(iconRect, icon);

					Widgets.DrawHighlight(new Rect(primeRect.xMin - 3f, primeRect.yMin - 3f, primeRect.width + 6f, primeRect.height + 6f));
				}

				primePositionVector.x += standardSpacing + draggableTextureDiameter;
			}

			DD_Widgets.BoxOutline(scrollViewBox);

			Widgets.BeginScrollView(scrollViewOutRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			for (int k = 0; k < this.cachedPawnSurfaces.Count; k++)
			{
				PawnSurface surface = this.cachedPawnSurfaces[k];

				Pawn pawn = surface.pawn;

				if (firstPawnDrawn)
				{
					DD_Widgets.ListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				Rect surfaceRect = new Rect(nameRect.xMax, currentVerticalPosition, pawnSurfaceWidth, standardRowHeight);
				float surfaceRectCenterY = surfaceRect.center.y;

				DD_Widgets.PawnLabel(nameRect, pawn, this.listMousePosition);

				for (int p = 0; p < DD_Widgets.PrimaryDraggablesList.Count; p++)
				{
					WorkTypeDef def = DD_Widgets.PrimaryDraggablesList[p].def;
					Rect drawRect = new Vector2(this.primePositions[def], surfaceRectCenterY).ToDraggableRect();
					int tooltipSelector = 1;

					//Pawn is assigned to the work type
					if (surface.QuickFindByDef.TryGetValue(def, out DraggableWorkType matchingDraggable))
					{
						matchingDraggable.DrawTexture(drawRect);
					}

					else
					{
						//Pawn is unassigned by incapability
						if (pawn.story.WorkTypeIsDisabled(def))
						{
							tooltipSelector = 2;
						}

						//Pawn is unassigned by player choice
						else
						{
							tooltipSelector = 3;

							DD_Widgets.DraggableOutline(drawRect, DD_Widgets.MediumSkillColour);

							DD_Widgets.DrawPassion(pawn, def, drawRect);
						}
					}

					if (drawRect.Contains(this.listMousePosition))
					{
						string tipString;

						switch (tooltipSelector)
						{
							case 1:
								tipString = DD_Widgets.DraggableTooltip(def, true, false, pawn);
								break;
							case 2:
								tipString = "DD_WorkTab_ColonistStats_WorkTypeForbidden".CachedTranslation().AdjustedFor(pawn);
								break;
							case 3:
								tipString = DD_Widgets.DraggableTooltip(def, true, false, pawn) + "DD_WorkTab_ColonistStats_CurrentlyUnassigned".CachedTranslation().AdjustedFor(pawn);
								break;
							default:
								tipString = string.Empty;
								break;
						}

						TooltipHandler.TipRegion(drawRect, tipString);
					}
				}

				currentVerticalPosition += standardRowHeight;
			}

			Widgets.EndScrollView();

#if DEBUG
			msProfile2.Stop();
			Log.Message($"ColonistsTab_Frame_{Time.frameCount}_{Event.current.type.ToString()}: {msProfile2.ElapsedMilliseconds}ms");
#endif
		}

		public override void PostClose()
		{
			base.PostClose();

			this.sortingDef = null;
			this.sortingOrder = SortOrder.Undefined;
		}

		public Window_ColonistStats(bool visibleMap) : base()
		{
			this.closeOnClickedOutside = true;
			DD_Settings.ColonistStatsOnlyVisibleMap = visibleMap;
		}
	}
}
