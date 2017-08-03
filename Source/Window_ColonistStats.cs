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
		private static float spaceForPawnLabel = DDUtilities.spaceForPawnLabel;

		private static float standardSpacing = DDUtilities.standardSpacing;

		private static float standardRowHeight = DDUtilities.standardRowHeight;

		private static float standardRowWidth = spaceForPawnLabel + DDUtilities.standardSurfaceWidth;

		private float preAdjustedWidth => standardRowWidth + 2 * standardSpacing + 2 * this.Margin + 20f;

		private float preAdjustedHeight => standardRowHeight * (1f + this.colonistsCount) + 2f * standardSpacing + 2 * this.Margin + 20f;

		private static float maxAllowedWidth = (float)UI.screenWidth - 10f;

		private static float maxAllowedHeight = (float)UI.screenHeight * 0.75f;

		private float listOffset => preAdjustedWidth > maxAllowedWidth ? this.scrollPosition.x : 0f;

		private PrimarySurface primeTypes = new PrimarySurface();

		private Vector2 scrollPosition = Vector2.zero;

		private IEnumerable<Pawn> colonistsToDraw;

		private bool needToRecachePawns = true;

		private WorkTypeDef sortingDef;

		private SortOrder sortingOrder = SortOrder.Undefined;

		private string toggleButtonText
		{
			get
			{
				if (Settings.ColonistStatsOnlyVisibleMap)
				{
					return "DD_WorkTab_ColonistStats_ToggleButton_VisibleMap".TranslateFast();
				}

				else return "DD_WorkTab_ColonistStats_ToggleButton_AllMaps".TranslateFast();
			}
		}

		private int colonistsCount
		{
			get
			{
				int count = 0;

				if (Settings.ColonistStatsOnlyVisibleMap)
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

				else return 1;
			}
		}

		private void RecachePawnList()
		{
			this.needToRecachePawns = false;

			IEnumerable<Pawn> pawnsList = Settings.ColonistStatsOnlyVisibleMap ? Find.VisibleMap.mapPawns.FreeColonists : PawnsFinder.AllMaps_FreeColonists;

			if (this.sortingOrder == SortOrder.Undefined || this.sortingDef == null)
			{
				pawnsList = pawnsList.OrderBy(p => DDUtilities.GetLabelForPawn(p));
			}

			else
			{
				pawnsList = pawnsList.OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef)).ThenByDescending(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).ThenBy(p => DDUtilities.GetLabelForPawn(p));

				if (this.sortingOrder == SortOrder.Ascending)
				{
					pawnsList = pawnsList.Reverse();
				}
			}

			this.colonistsToDraw = pawnsList.ToArray();
		}

		public override Vector2 InitialSize
		{
			get
			{
				float width = preAdjustedWidth;
				float height = preAdjustedHeight;

				if (preAdjustedWidth > maxAllowedWidth)
				{
					width = maxAllowedWidth;
				}

				if (height > maxAllowedHeight)
				{
					height = maxAllowedHeight;
				}

				return new Vector2(width, height);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.SetInitialSizeAndPosition();

			if (needToRecachePawns)
			{
				this.RecachePawnList();
			}

			//General Rects
			Rect toggleMapRect = new Rect(inRect.x - this.listOffset, inRect.y, standardSpacing + spaceForPawnLabel, standardRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, inRect.y, inRect.width - toggleMapRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - primaryTypesRect.height);
			Rect scrollViewOutRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, standardRowWidth, this.colonistsCount * standardRowHeight);

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(standardSpacing), this.toggleButtonText, true, false, true))
			{
				Settings.ColonistStatsOnlyVisibleMap = !Settings.ColonistStatsOnlyVisibleMap;

				this.needToRecachePawns = true;

				if (Settings.UseSounds)
				{
					SoundDefOf.DialogBoxAppear.PlayOneShotOnCamera(null);
				}
			}

			Vector2 primePosition = new Vector2(primaryTypesRect.x + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f), primaryTypesRect.center.y);

			for (int i = 0; i < this.primeTypes.PrimeDraggablesList.Count; i++)
			{
				DraggableWorkType prime = this.primeTypes.PrimeDraggablesList[i];

				prime.position = primePosition;

				Rect primeRect = primePosition.ToDraggableRect();

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
								this.sortingOrder = this.sortingOrder.Next();
							}
						}

						else if (Event.current.button == 1)
						{
							if (this.sortingDef != prime.def)
							{
								this.sortingDef = prime.def;
								this.sortingOrder = SortOrder.Ascending;
							}

							else
							{
								this.sortingOrder = this.sortingOrder.Previous();
							}
						}

						Event.current.Use();

						if (Settings.UseSounds)
						{
							SoundDef soundToUse = this.sortingOrder == SortOrder.Undefined ? SoundDefOf.TickLow : SoundDefOf.TickHigh;

							soundToUse.PlayOneShotOnCamera(null);
						}
					}

					else
					{
						Widgets.DrawHighlight(primeRect);

						TooltipHandler.TipRegion(primeRect, DDUtilities.GetDraggableTooltip(prime.def, true, true, null));
					}
				}


				if (this.sortingOrder != SortOrder.Undefined && this.sortingDef == prime.def)
				{
					//Draw little arrow indicator below work type
					Texture2D icon = this.sortingOrder == SortOrder.Descending ? DDUtilities.SortingDescendingIcon : DDUtilities.SortingIcon;
					Rect iconRect = new Rect(primeRect.xMax - (float)icon.width, primeRect.yMax + 1f, icon.width, icon.height);

					GUI.DrawTexture(iconRect, icon);

					//Highlight column
					Rect highlightRect = new Rect(primeRect.xMin - 2f, primeRect.yMin - standardSpacing / 2f, primeRect.width + 4f, inRect.height - 2f * standardSpacing);

					Widgets.DrawHighlight(highlightRect);
				}

				primePosition.x += standardSpacing + DDUtilities.DraggableTextureWidth;
			}

			//Draw rect edges
			DDUtilities.DrawOutline(scrollViewBox, false, true);

			Widgets.BeginScrollView(scrollViewOutRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (Pawn pawn in this.colonistsToDraw)
			{
				PawnSurface pawnSurface = Dragger.GetPawnSurface(pawn);

				//List separator
				if (firstPawnDrawn)
				{
					DDUtilities.DrawListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				//Pawn name
				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				DDUtilities.DoPawnLabel(nameRect, pawn);

				//Work types
				Rect surfaceRect = new Rect(nameRect.xMax, currentVerticalPosition, scrollViewInnerRect.width - nameRect.width, standardRowHeight);

				for (int i = 0; i < this.primeTypes.PrimeDraggablesList.Count; i++)
				{
					DraggableWorkType currentPrime = this.primeTypes.PrimeDraggablesList[i];
					DraggableWorkType matchingDraggable = pawnSurface.childrenListForReading.Find(d => d.def == currentPrime.def);
					Rect drawRect = new Vector2(currentPrime.position.x, surfaceRect.center.y).ToDraggableRect();
					int tooltipSelector;

					//Pawn is assigned to the work type
					if (matchingDraggable != null)
					{
						tooltipSelector = 1;

						matchingDraggable.DrawTexture(drawRect);
					}

					else
					{
						//Pawn is unassigned by incapability
						if (pawn.story.WorkTypeIsDisabled(currentPrime.def))
						{
							tooltipSelector = 2;

							GUI.DrawTexture(drawRect, BaseContent.BadTex);
						}

						//Pawn is unassigned by player choice
						else
						{
							tooltipSelector = 3;

							GUI.DrawTexture(drawRect, DDUtilities.HaltIcon);
						}
					}

					if (Mouse.IsOver(drawRect))
					{
						string tipString;

						switch (tooltipSelector)
						{
							case 1:
								tipString = DDUtilities.GetDraggableTooltip(matchingDraggable.def, true, false, matchingDraggable.parent.pawn);
								break;
							case 2:
								tipString = "DD_WorkTab_ColonistStats_WorkTypeForbidden".TranslateFast(new string[] { currentPrime.def.gerundLabel }).AdjustedFor(pawn);
								break;
							case 3:
								tipString = DDUtilities.GetDraggableTooltip(currentPrime.def, true, false, pawn) + "\n\n" + "DD_WorkTab_ColonistStats_CurrentlyUnassigned".TranslateFast(new string[] { currentPrime.def.gerundLabel }).AdjustedFor(pawn);
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

		public Window_ColonistStats(bool visibleMap)
		{
			this.layer = WindowLayer.GameUI;
			this.doCloseX = true;
			this.closeOnClickedOutside = true;

			Settings.ColonistStatsOnlyVisibleMap = visibleMap;
		}
	}
}
