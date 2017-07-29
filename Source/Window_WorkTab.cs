using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_WorkTab : MainTabWindow
	{
		private static float spaceForPawnLabel = DDUtilities.spaceForPawnLabel;

		private static float spaceForWorkButtons = DDUtilities.spaceForWorkButtons;

		private static float standardSpacing = DDUtilities.standardSpacing;

		private static float standardRowHeight = DDUtilities.standardRowHeight;

		private static float standardSurfaceWidth = DDUtilities.standardSurfaceWidth;

		private PrimarySurface primarySurface = new PrimarySurface();

		private Vector2 scrollPosition = Vector2.zero;

		private bool shouldResetDrag = false;

		private int totalColonists
		{
			get
			{
				int count = Find.VisibleMap.mapPawns.FreeColonistsCount;

				if (count > 0)
				{
					return count;
				}

				else return 1;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			Text.Font = GameFont.Tiny;
			float spaceForIndicators = Text.LineHeight;

			//General Rects
			Rect indicatorsRect = new Rect(inRect.x + standardSpacing + spaceForPawnLabel + spaceForWorkButtons, inRect.y, inRect.width - 2 * standardSpacing - spaceForPawnLabel - spaceForWorkButtons, spaceForIndicators);
			Rect primarySurfaceRect = new Rect(inRect.x - this.listOffset, indicatorsRect.yMax, inRect.width, standardRowHeight);
			Rect scrollViewBox = new Rect(inRect.x, primarySurfaceRect.yMax, inRect.width, inRect.height - indicatorsRect.height - primarySurfaceRect.height);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(standardSpacing);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, spaceForPawnLabel + spaceForWorkButtons + standardSurfaceWidth, totalColonists * standardRowHeight);

			//Draw priority indicators
			this.DrawIndicators(indicatorsRect);

			//Primary work types
			primarySurface.OnGUI(primarySurfaceRect);

			//Draw rect edges
			DDUtilities.DrawOutline(scrollViewBox, false);

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;
			bool firstPawnDrawn = false;

			foreach (Pawn pawn in Find.VisibleMap.mapPawns.FreeColonists)
			{
				PawnSurface surface = Dragger.SurfaceForPawn(pawn);

				//List separator
				if (firstPawnDrawn)
				{
					DDUtilities.DrawListSeparator(scrollViewInnerRect, currentVerticalPosition);
				}

				firstPawnDrawn = true;

				//Rects
				Rect pawnLabelRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnLabel, standardRowHeight);
				Rect disableAllRect = new Vector2(pawnLabelRect.xMax + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f), pawnLabelRect.center.y).ToDraggableRect();
				Rect resetToVanillaRect = new Vector2(disableAllRect.xMax + standardSpacing + (DDUtilities.DraggableTextureWidth / 2f), disableAllRect.center.y).ToDraggableRect();
				Rect surfaceRect = new Rect(pawnLabelRect.xMax + spaceForWorkButtons, currentVerticalPosition, scrollViewInnerRect.width - pawnLabelRect.width - spaceForWorkButtons, standardRowHeight);

				//Pawn name
				DDUtilities.DoPawnLabel(pawnLabelRect, pawn);

				//Disable All Work button
				this.ButtonDisableAll(surface, disableAllRect);

				//Reset to Vanilla button
				this.ButtonResetToVanilla(surface, resetToVanillaRect);

				//Check for drag, draw draggables
				surface.OnGUI(surfaceRect, this.scrollPosition);

				//Increment list y for next pawn
				currentVerticalPosition += standardRowHeight;
			}

			Widgets.EndScrollView();

			//Check for invalid drop
			if (this.shouldResetDrag)
			{
				Dragger.CurrentDraggingObj.Clear();
				this.shouldResetDrag = false;
			}

			if (!Input.anyKey && Event.current.type != EventType.MouseUp && Dragger.Dragging)
			{
				this.shouldResetDrag = true;
			}
		}

		private void DrawIndicators(Rect rect)
		{
			GUI.color = new Color(1f, 1f, 1f, 0.5f);

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<= " + "HigherPriority".Translate());

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "LowerPriority".Translate() + " =>");

			GUI.color = Color.white; // Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		//Button: set all pawn priorities to 0 (disabled)
		private void ButtonDisableAll(PawnSurface surface, Rect buttonRect)
		{
			DDUtilities.DrawOutline(buttonRect, false);

			Widgets.DrawHighlightIfMouseover(buttonRect);

			TooltipHandler.TipRegion(buttonRect, "DD_WorkTab_ButtonDisableAll_Tooltip".Translate());

			GUI.color = DDUtilities.ButtonColour;
			GUI.DrawTexture(buttonRect.ContractedBy(2f), DDUtilities.ButtonTexture_DisableAll);
			GUI.color = Color.white; //Reset

			if (Widgets.ButtonInvisible(buttonRect, false))
			{
				if (Settings.ShowPrompt)
				{
					DiaOption acceptButton = new DiaOption("DD_WorkTab_ButtonOption_Accept".Translate());
					acceptButton.action = delegate
					{
						surface.DisableAllWork();
					};
					acceptButton.resolveTree = true;

					DiaOption rejectButton = new DiaOption("DD_WorkTab_ButtonOption_Cancel".Translate());
					rejectButton.resolveTree = true;

					DiaOption acceptDoNotShowAgain = new DiaOption("DD_WorkTab_ButtonOption_AcceptDisablePrompt".Translate());
					acceptDoNotShowAgain.action = delegate
					{
						Settings.ShowPrompt = false;

						surface.DisableAllWork();
					};
					acceptDoNotShowAgain.resolveTree = true;

					DiaNode prompt = new DiaNode("DD_WorkTab_ButtonDisableAll_Text".Translate().AdjustedFor(surface.pawn) + "DD_WorkTab_ButtonText_DisablePrompt".Translate());
					prompt.options.Add(acceptButton);
					prompt.options.Add(rejectButton);
					prompt.options.Add(acceptDoNotShowAgain);

					Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, "DD_WorkTab_ButtonDisableAll_Title".Translate().AdjustedFor(surface.pawn)));
				}

				else
				{
					surface.DisableAllWork();
				}
			}
		}

		//Button: reset all pawn priorities according to natural work type importance
		private void ButtonResetToVanilla(PawnSurface surface, Rect buttonRect)
		{
			DDUtilities.DrawOutline(buttonRect, false);

			Widgets.DrawHighlightIfMouseover(buttonRect);

			TooltipHandler.TipRegion(buttonRect, "DD_WorkTab_ButtonResetVanilla_Tooltip".Translate());

			GUI.color = DDUtilities.ButtonColour;
			GUI.DrawTexture(buttonRect.ContractedBy(2f), DDUtilities.ButtonTexture_ResetToVanilla);
			GUI.color = Color.white; //Reset

			if (Widgets.ButtonInvisible(buttonRect, false))
			{
				if (Settings.ShowPrompt)
				{
					DiaOption acceptButton = new DiaOption("DD_WorkTab_ButtonOption_Accept".Translate());
					acceptButton.action = delegate
					{
						surface.ResetToVanillaSettings();
					};
					acceptButton.resolveTree = true;

					DiaOption rejectButton = new DiaOption("DD_WorkTab_ButtonOption_Cancel".Translate());
					rejectButton.resolveTree = true;

					DiaOption acceptDoNotShowAgain = new DiaOption("DD_WorkTab_ButtonOption_AcceptDisablePrompt".Translate());
					acceptDoNotShowAgain.action = delegate
					{
						Settings.ShowPrompt = false;

						surface.ResetToVanillaSettings();
					};
					acceptDoNotShowAgain.resolveTree = true;

					DiaNode prompt = new DiaNode("DD_WorkTab_ButtonResetVanilla_Text".Translate().AdjustedFor(surface.pawn) + "DD_WorkTab_ButtonText_DisablePrompt".Translate());
					prompt.options.Add(acceptButton);
					prompt.options.Add(rejectButton);
					prompt.options.Add(acceptDoNotShowAgain);

					Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, "DD_WorkTab_ButtonResetVanilla_Title".Translate().AdjustedFor(surface.pawn)));
				}

				else
				{
					surface.ResetToVanillaSettings();
				}
			}
		}

		//General Window settings
		public Window_WorkTab()
		{
			Current.Game.playSettings.useWorkPriorities = true;
		}

		public override bool CausesMessageBackground()
		{
			return true;
		}

		private float preAdjustedWidth => spaceForPawnLabel + spaceForWorkButtons + standardSurfaceWidth + 2 * standardSpacing + 2 * this.Margin + 20f;
		private float preAdjustedHeight => standardRowHeight * (1 + this.totalColonists) + 2 * standardSpacing + 2 * this.Margin + 20f;
		private static float maxAllowedWidth = (float)UI.screenWidth - 10f;
		private static float maxAllowedHeight = (float)UI.screenHeight * 0.75f;
		private float listOffset => preAdjustedWidth > maxAllowedWidth ? this.scrollPosition.x : 0f;

		public override Vector2 RequestedTabSize
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

		public override void PostClose()
		{
			base.PostClose();

			Dragger.CurrentDraggingObj.Clear();
		}
	}
}
