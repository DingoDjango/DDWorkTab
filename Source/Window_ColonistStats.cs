using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_ColonistStats : Window
	{
		public const float defaultWidth = 1250f;

		public const float defaultHeight = 700f;

		public const float standardSpacing = Window_WorkTab.spaceBetweenTypes;

		public const float rowHeight = Window_WorkTab.surfaceHeight;

		public const float spaceForPawnName = Window_WorkTab.spaceForPawnName;

		public static float rowWidth = Window_WorkTab.spaceForPawnName + Window_WorkTab.surfaceWidth;

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
					return Find.VisibleMap.mapPawns.FreeColonistsCount;
				}

				else
				{
					List<Map> allMaps = Find.Maps;
					int count = 0;

					for (int i = 0; i < allMaps.Count; i++)
					{
						count += allMaps[i].mapPawns.FreeColonistsCount;
					}

					return count;
				}
			}
		}

		private IEnumerable<Pawn> colonistsToDraw
		{
			get
			{
				if (Settings.ColonistStatsOnlyVisibleMap)
				{
					if (this.sortingOrder == SortOrder.Descending && this.sortingDef != null)
					{
						return Find.VisibleMap.mapPawns.FreeColonists.OrderByDescending(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef));
					}

					else if (this.sortingOrder == SortOrder.Ascending && this.sortingDef != null)
					{
						return Find.VisibleMap.mapPawns.FreeColonists.OrderBy(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef));
					}

					else return Find.VisibleMap.mapPawns.FreeColonists;
				}

				else
				{
					if (this.sortingOrder == SortOrder.Descending && this.sortingDef != null)
					{
						return PawnsFinder.AllMaps_FreeColonists.OrderByDescending(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef));
					}

					else if (this.sortingOrder == SortOrder.Ascending && this.sortingDef != null)
					{
						return PawnsFinder.AllMaps_FreeColonists.OrderBy(p => p.skills.AverageOfRelevantSkillsFor(this.sortingDef)).OrderBy(p => p.story.WorkTypeIsDisabled(this.sortingDef));
					}

					else return PawnsFinder.AllMaps_FreeColonists;
				}
			}
		}

		private float windowWidth
		{
			get
			{
				float widthTotal = (this.Margin * 2f) + (standardSpacing * 2f) + rowWidth;

				if (widthTotal < defaultWidth)
				{
					return widthTotal;
				}

				else return defaultWidth;
			}
		}

		private float windowHeight
		{
			get
			{
				float heightTotal = (this.Margin * 2f) + (standardSpacing * 2f) + rowHeight * (1f + this.colonistsCount);

				if (heightTotal < defaultHeight)
				{
					return heightTotal;
				}

				else return defaultHeight;
			}
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(this.windowWidth, this.windowHeight);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.SetInitialSizeAndPosition();

			//General Rects
			Rect toggleMapRect = new Rect(inRect.x - this.scrollPosition.x, inRect.y, standardSpacing + spaceForPawnName, rowHeight);
			Rect primaryTypesRect = new Rect(toggleMapRect.xMax, inRect.y, inRect.width - toggleMapRect.width, rowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primaryTypesRect.yMax, inRect.width, inRect.height - primaryTypesRect.height);
			Rect scrollViewOutRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, rowWidth, this.colonistsCount * rowHeight);

			if (Widgets.ButtonText(toggleMapRect.ContractedBy(standardSpacing), this.toggleButtonText, true, false, true))
			{
				Settings.ColonistStatsOnlyVisibleMap = !Settings.ColonistStatsOnlyVisibleMap;
			}

			Vector2 primePosition = new Vector2(primaryTypesRect.x + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f), primaryTypesRect.center.y);

			foreach (DraggableWorkType prime in this.primeTypes.PrimeDraggablesByPriority)
			{
				prime.position = primePosition;

				Rect primeRect = primePosition.ToDraggableRect();

				prime.DrawDraggableTexture(primeRect);

				Widgets.DrawHighlightIfMouseover(primeRect);

				TooltipHandler.TipRegion(primeRect, prime.def.GetDraggableTooltip(true, null) + "\n\n" + "DD_WorkTab_ColonistStats_SortingTip".Translate());

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

					if (Event.current.button == 1)
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
					Texture2D texture = BaseContent.BadTex;
					Rect iconRect = new Rect();

					if (this.sortingOrder == SortOrder.Descending)
					{
						texture = DDUtilities.SortingDescendingIcon;
						iconRect = new Rect(primeRect.xMax - (float)texture.width, primeRect.yMax + 1f, texture.width, texture.height);
					}

					else if (this.sortingOrder == SortOrder.Ascending)
					{
						texture = DDUtilities.SortingIcon;
						iconRect = new Rect(primeRect.xMax - (float)texture.width, primeRect.yMax + 1f, texture.width, texture.height);
					}

					GUI.DrawTexture(iconRect, texture);

					float highlightHeight = (this.windowHeight < defaultHeight) ? (this.colonistsCount + 1) * rowHeight : inRect.height - (standardSpacing * 5f);
					Rect highlightRect = new Rect(primeRect.xMin - 2f, primeRect.yMin - 2f, primeRect.width + 4f, highlightHeight);
					Widgets.DrawHighlight(highlightRect);
				}

				primePosition.x += standardSpacing + DDUtilities.DraggableTextureWidth;
			}

			//Draw rect edges
			DDUtilities.DrawOutline(scrollViewBox);

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
				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnName, rowHeight);
				this.DoPawnLabel(nameRect, pawn);

				//Work types / draggables
				Rect surfaceRect = new Rect(nameRect.xMax, currentVerticalPosition, scrollViewInnerRect.width - nameRect.width, rowHeight);

				for (int i = 0; i < this.primeTypes.PrimeDraggablesList.Count; i++)
				{
					DraggableWorkType currentPrime = this.primeTypes.PrimeDraggablesList[i];
					DraggableWorkType matchingDraggable = pawnSurface.childrenListForReading.Find(d => d.def == currentPrime.def);
					Rect drawRect = new Vector2(surfaceRect.x + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f) + i * (standardSpacing + DDUtilities.DraggableTextureWidth), surfaceRect.center.y).ToDraggableRect();

					Widgets.DrawHighlightIfMouseover(drawRect);

					//Pawn is assigned to the work type
					if (matchingDraggable != null)
					{
						matchingDraggable.DrawDraggableTexture(drawRect);

						TooltipHandler.TipRegion(drawRect, matchingDraggable.def.GetDraggableTooltip(false, matchingDraggable.parent.pawn));
					}

					else
					{
						//Pawn is unassigned by incapability
						if (pawn.story.WorkTypeIsDisabled(currentPrime.def))
						{
							GUI.DrawTexture(drawRect, BaseContent.BadTex);

							TooltipHandler.TipRegion(drawRect, "DD_WorkTab_PawnSurface_WorkTypeForbidden".Translate(new object[] { currentPrime.def.labelShort }).AdjustedFor(pawn));
						}

						//Pawn is unassigned by player choice
						else
						{
							GUI.DrawTexture(drawRect, DDUtilities.HaltIcon);

							string tip = currentPrime.def.GetDraggableTooltip(false, pawn) + "\n\n" + "DD_WorkTab_ColonistStats_CurrentlyUnassigned".Translate(new object[] { currentPrime.def.labelShort }).AdjustedFor(pawn);

							TooltipHandler.TipRegion(drawRect, tip);
						}
					}
				}

				currentVerticalPosition += rowHeight;
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
