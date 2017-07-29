using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_ColonistStats : Window
	{
		private static float spaceForPawnLabel = DDUtilities.spaceForPawnLabel;

		private static float standardSpacing = DDUtilities.standardSpacing;

		private static float standardRowHeight = DDUtilities.standardRowHeight;

		private static float standardRowWidth = spaceForPawnLabel + DDUtilities.standardSurfaceWidth;

		private PrimarySurface primeTypes = new PrimarySurface();

		private Vector2 scrollPosition = Vector2.zero;

		private WorkTypeDef sortingDef;

		private SortOrder sortingOrder = SortOrder.Undefined;

		private string toggleButtonText
		{
			get
			{
				if (Settings.ColonistStatsOnlyVisibleMap)
				{
					return "DD_WorkTab_ColonistStats_ToggleButton_VisibleMap".Translate();
				}

				else return "DD_WorkTab_ColonistStats_ToggleButton_AllMaps".Translate();
			}
		}

		private int colonistsCount
		{
			get
			{
				if (Settings.ColonistStatsOnlyVisibleMap)
				{
					int countVisible = Find.VisibleMap.mapPawns.FreeColonistsCount;

					if (countVisible > 0)
					{
						return countVisible;
					}

					else return 1;
				}

				else
				{
					List<Map> allMaps = Find.Maps;
					int count = 0;

					for (int i = 0; i < allMaps.Count; i++)
					{
						count += allMaps[i].mapPawns.FreeColonistsCount;
					}

					if (count > 0)
					{
						return count;
					}

					else return 1;
				}
			}
		}

		private IEnumerable<Pawn> colonistsToDraw
		{
			get
			{
				IEnumerable<Pawn> pawnsList = Settings.ColonistStatsOnlyVisibleMap ? Find.VisibleMap.mapPawns.FreeColonists : PawnsFinder.AllMaps_FreeColonists;

				if (this.sortingOrder == SortOrder.Descending && this.sortingDef != null)
				{
					return pawnsList.OrderByDescending(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef));
				}

				else if (this.sortingOrder == SortOrder.Ascending && this.sortingDef != null)
				{
					return pawnsList.OrderBy(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).OrderBy(p => !p.story.WorkTypeIsDisabled(this.sortingDef));
				}

				else return pawnsList;
			}
		}

		private float preAdjustedWidth => standardRowWidth + 2 * standardSpacing + 2 * this.Margin + 20f;
		private float preAdjustedHeight => standardRowHeight * (1f + this.colonistsCount) + 2f * standardSpacing + 2 * this.Margin + 20f;
		private static float maxAllowedWidth = (float)UI.screenWidth - 10f;
		private static float maxAllowedHeight = (float)UI.screenHeight * 0.75f;
		private float listOffset => preAdjustedWidth > maxAllowedWidth ? this.scrollPosition.x : 0f;

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

			//General Rects
			Rect toggleMapRect = new Rect(inRect.x - this.listOffset, inRect.y, standardSpacing + spaceForPawnLabel, standardRowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, inRect.y, inRect.width - toggleMapRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - primaryTypesRect.height);
			Rect scrollViewOutRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, standardRowWidth, this.colonistsCount * standardRowHeight);

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(standardSpacing), this.toggleButtonText, true, false, true))
			{
				Settings.ColonistStatsOnlyVisibleMap = !Settings.ColonistStatsOnlyVisibleMap;
			}

			Vector2 primePosition = new Vector2(primaryTypesRect.x + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f), primaryTypesRect.center.y);

			foreach (DraggableWorkType prime in this.primeTypes.PrimeDraggablesByPriority)
			{
				prime.position = primePosition;

				Rect primeRect = primePosition.ToDraggableRect();

				prime.DrawTexture(primeRect, true);

				TooltipHandler.TipRegion(primeRect, prime.GetDraggableTooltip(true));

				if (Mouse.IsOver(primeRect) && Event.current.type == EventType.MouseDown)
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

						Event.current.Use();
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

						Event.current.Use();
					}
				}

				//Draw little arrow indicator below work type and highlight column
				if (this.sortingDef == prime.def && this.sortingOrder != SortOrder.Undefined)
				{
					if (this.sortingOrder == SortOrder.Descending)
					{
						Texture2D iconDescending = DDUtilities.SortingDescendingIcon;
						Rect iconRect = new Rect(primeRect.xMax - (float)iconDescending.width, primeRect.yMax + 1f, iconDescending.width, iconDescending.height);

						GUI.DrawTexture(iconRect, iconDescending);
					}

					else if (this.sortingOrder == SortOrder.Ascending)
					{
						Texture2D iconAscending = DDUtilities.SortingIcon;
						Rect iconRect = new Rect(primeRect.xMax - (float)iconAscending.width, primeRect.yMax + 1f, iconAscending.width, iconAscending.height);

						GUI.DrawTexture(iconRect, iconAscending);
					}

					float highlightHeight = inRect.height - 2f * standardSpacing;
					Rect highlightRect = new Rect(primeRect.xMin - 2f, primeRect.yMin - standardSpacing / 2f, primeRect.width + 4f, highlightHeight);
					Widgets.DrawHighlight(highlightRect);
				}

				primePosition.x += standardSpacing + DDUtilities.DraggableTextureWidth;
			}

			//Draw rect edges
			DDUtilities.DrawOutline(scrollViewBox, false);

			Widgets.BeginScrollView(scrollViewOutRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (Pawn pawn in this.colonistsToDraw)
			{
				PawnSurface pawnSurface = Dragger.SurfaceForPawn(pawn);

				//List separator
				if (firstPawnDrawn)
				{
					DDUtilities.DrawListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				//Pawn name
				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				DDUtilities.DoPawnLabel(nameRect, pawn);

				//Work types / draggables
				Rect surfaceRect = new Rect(nameRect.xMax, currentVerticalPosition, scrollViewInnerRect.width - nameRect.width, standardRowHeight);

				for (int i = 0; i < this.primeTypes.PrimeDraggablesList.Count; i++)
				{
					DraggableWorkType currentPrime = this.primeTypes.PrimeDraggablesList[i];
					DraggableWorkType matchingDraggable = pawnSurface.childrenListForReading.Find(d => d.def == currentPrime.def);
					Rect drawRect = new Vector2(surfaceRect.x + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f) + i * (standardSpacing + DDUtilities.DraggableTextureWidth), surfaceRect.center.y).ToDraggableRect();

					//Pawn is assigned to the work type
					if (matchingDraggable != null)
					{
						matchingDraggable.DrawTexture(drawRect, true);

						TooltipHandler.TipRegion(drawRect, matchingDraggable.GetDraggableTooltip(true));
					}

					else
					{
						//Pawn is unassigned by incapability
						if (pawn.story.WorkTypeIsDisabled(currentPrime.def))
						{
							GUI.DrawTexture(drawRect, BaseContent.BadTex);

							TooltipHandler.TipRegion(drawRect, "DD_WorkTab_PawnSurface_WorkTypeForbidden".Translate(new string[] { currentPrime.def.gerundLabel }).AdjustedFor(pawn));
						}

						//Pawn is unassigned by player choice
						else
						{
							GUI.DrawTexture(drawRect, DDUtilities.HaltIcon);

							DraggableWorkType temporaryDraggable = new DraggableWorkType(pawnSurface, currentPrime.def, -1);

							string tip = temporaryDraggable.GetDraggableTooltip(true) + "\n\n" + "DD_WorkTab_ColonistStats_CurrentlyUnassigned".Translate(new string[] { temporaryDraggable.def.gerundLabel }).AdjustedFor(pawn);

							TooltipHandler.TipRegion(drawRect, tip);
						}
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

		public Window_ColonistStats()
		{
			this.layer = WindowLayer.GameUI;
			this.doCloseX = true;
			this.closeOnClickedOutside = true;
		}

		public Window_ColonistStats(bool visibleMap) : this()
		{
			Settings.ColonistStatsOnlyVisibleMap = visibleMap;
		}
	}
}
