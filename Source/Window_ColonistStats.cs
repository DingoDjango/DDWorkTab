using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab
{
	public class Window_ColonistStats : Window
	{
		private const float draggableTextureDiameter = DD_Widgets.DraggableTextureDiameter;

		private const float spaceForPawnLabel = DD_Widgets.SpaceForPawnLabel;

		private const float spaceForWorkButtons = DD_Widgets.SpaceForWorkButtons;

		private const float standardSpacing = DD_Widgets.StandardSpacing;

		private const float standardRowHeight = DD_Widgets.StandardRowHeight;

		private static readonly float standardSurfaceWidth = DD_Widgets.StandardSurfaceWidth;

		private static readonly float rowWidth = spaceForPawnLabel + standardSurfaceWidth;

		private Dictionary<WorkTypeDef, Vector2> primePositions = new Dictionary<WorkTypeDef, Vector2>();

		private Vector2 scrollPosition = Vector2.zero;

		private IEnumerable<Pawn> colonistsToDraw;

		private bool mustRecachePawns = true;

		private WorkTypeDef sortingDef;

		private SortOrder sortingOrder = SortOrder.Undefined;

		private float PreAdjustedWidth => rowWidth + 2 * standardSpacing + 2 * this.Margin + 20f;

		private float PreAdjustedHeight => standardRowHeight * (1f + this.ColonistsCount) + 2f * standardSpacing + 2 * this.Margin + 20f;

		private float ListOffset => PreAdjustedWidth > DD_Widgets.MaxWindowWidth ? this.scrollPosition.x : 0f;

		private string ToggleButtonText
		{
			get
			{
				if (DD_Settings.ColonistStatsOnlyVisibleMap)
				{
					return "DD_WorkTab_ColonistStats_ToggleButton_VisibleMap".CachedTranslation();
				}

				return "DD_WorkTab_ColonistStats_ToggleButton_AllMaps".CachedTranslation();
			}
		}

		private int ColonistsCount
		{
			get
			{
				int count = 0;

				if (DD_Settings.ColonistStatsOnlyVisibleMap)
				{
					count = Find.VisibleMap.mapPawns.FreeColonistsCount;
				}

				else
				{
					List<Map> allMaps = Find.Maps;

					for (int i = 0; i < allMaps.Count; i++)
					{
						count += allMaps[i].mapPawns.FreeColonistsCount;
					}
				}

				if (count > 0)
				{
					return count;
				}

				return 1;
			}
		}

		private void RecachePawnList()
		{
			this.mustRecachePawns = false;

			IEnumerable<Pawn> pawnsList = DD_Settings.ColonistStatsOnlyVisibleMap ? Find.VisibleMap.mapPawns.FreeColonists : PawnsFinder.AllMaps_FreeColonists;

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

			this.colonistsToDraw = pawnsList.ToArray();
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.SetInitialSizeAndPosition();

			if (mustRecachePawns)
			{
				this.RecachePawnList();
			}

			Rect toggleMapRect = new Rect(inRect.x - this.ListOffset, inRect.y, standardSpacing + spaceForPawnLabel, standardRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, inRect.y, inRect.width - toggleMapRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - primaryTypesRect.height);
			Rect scrollViewOutRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, rowWidth, this.ColonistsCount * standardRowHeight);

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(standardSpacing), this.ToggleButtonText, true, false, true))
			{
				DD_Settings.ColonistStatsOnlyVisibleMap = !DD_Settings.ColonistStatsOnlyVisibleMap;

				this.mustRecachePawns = true;

				if (DD_Settings.UseSounds)
				{
					SoundDefOf.DialogBoxAppear.PlayOneShotOnCamera(null);
				}
			}

			Vector2 primePositionVector = new Vector2(primaryTypesRect.x + standardSpacing + (draggableTextureDiameter / 2f), primaryTypesRect.center.y);

			for (int i = 0; i < PrimaryTypes.PrimaryDraggablesList.Count; i++)
			{
				DraggableWorkType prime = PrimaryTypes.PrimaryDraggablesList[i];

				primePositions[prime.def] = primePositionVector;

				Rect primeRect = primePositionVector.ToDraggableRect();

				prime.DrawTexture(primeRect);

				if (Mouse.IsOver(primeRect))
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

						this.mustRecachePawns = true;

						Event.current.Use();
					}

					Widgets.DrawHighlight(primeRect);

					TooltipHandler.TipRegion(primeRect, DD_Widgets.GetDraggableTooltip(prime.def, true, true, null));
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

			DD_Widgets.DrawBoxOutline(scrollViewBox);

			Widgets.BeginScrollView(scrollViewOutRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (Pawn pawn in this.colonistsToDraw)
			{
				PawnSurface pawnSurface = DragManager.GetPawnSurface(pawn);

				if (firstPawnDrawn)
				{
					DD_Widgets.DrawListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				Rect surfaceRect = new Rect(nameRect.xMax, currentVerticalPosition, standardSurfaceWidth, standardRowHeight);

				DD_Widgets.DrawPawnLabel(nameRect, pawn);

				for (int i = 0; i < PrimaryTypes.PrimaryDraggablesList.Count; i++)
				{
					WorkTypeDef def = PrimaryTypes.PrimaryDraggablesList[i].def;
					Rect drawRect = new Vector2(primePositions[def].x, surfaceRect.center.y).ToDraggableRect();
					int tooltipSelector;

					DD_Widgets.DrawPassion(pawn, def, drawRect);

					//Pawn is assigned to the work type
					if (pawnSurface.QuickFindByDef.TryGetValue(def, out DraggableWorkType matchingDraggable))
					{
						tooltipSelector = 1;

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
						}
					}

					if (Mouse.IsOver(drawRect))
					{
						string tipString;

						switch (tooltipSelector)
						{
							case 1:
								tipString = DD_Widgets.GetDraggableTooltip(def, true, false, pawn);
								break;
							case 2:
								tipString = "DD_WorkTab_ColonistStats_WorkTypeForbidden".CachedTranslation(new string[] { def.gerundLabel }).AdjustedFor(pawn);
								break;
							case 3:
								tipString = DD_Widgets.GetDraggableTooltip(def, true, false, pawn) + "DD_WorkTab_ColonistStats_CurrentlyUnassigned".CachedTranslation(new string[] { def.gerundLabel }).AdjustedFor(pawn);
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
		}

		public override void PostClose()
		{
			base.PostClose();

			this.sortingDef = null;
			this.sortingOrder = SortOrder.Undefined;
		}

		public override Vector2 InitialSize
		{
			get
			{
				float width = PreAdjustedWidth;
				float height = PreAdjustedHeight;

				if (PreAdjustedWidth > DD_Widgets.MaxWindowWidth)
				{
					width = DD_Widgets.MaxWindowWidth;
				}

				if (height > DD_Widgets.MaxWindowHeight)
				{
					height = DD_Widgets.MaxWindowHeight;
				}

				return new Vector2(width, height);
			}
		}

		public Window_ColonistStats(bool visibleMap)
		{
			this.layer = WindowLayer.GameUI;
			this.doCloseX = true;
			this.closeOnClickedOutside = true;

			DD_Settings.ColonistStatsOnlyVisibleMap = visibleMap;
		}
	}
}
