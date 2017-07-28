using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class Window_WorkTab : MainTabWindow
	{
		public const float spaceForPawnName = 140f;

		public const float spaceForButtons = 80f;

		public const float spaceBetweenTypes = 5f;

		public const float surfaceHeight = 2f * spaceBetweenTypes + DDUtilities.DraggableTextureHeight;

		public static float surfaceWidth = spaceBetweenTypes + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DDUtilities.DraggableTextureWidth + spaceBetweenTypes);

		private PrimarySurface primarySurface = new PrimarySurface();

		private Vector2 scrollPosition = Vector2.zero;

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

		private float totalRenderWidth
		{
			get
			{
				return spaceForPawnName + spaceForButtons + surfaceWidth + 60f;
			}
		}

		private float totalRenderHeight
		{
			get
			{
				return surfaceHeight * (1 + this.totalColonists) + 60f; //Accounted for mainTypesRect
			}
		}

		public Vector2 ScrollPosition
		{
			get
			{
				return this.scrollPosition;
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			Text.Font = GameFont.Tiny;
			float spaceForIndicators = Text.LineHeight;

			//General Rects
			Rect indicatorsRect = new Rect(inRect.x + spaceForPawnName + spaceForButtons, inRect.y, inRect.width - spaceForPawnName - spaceForButtons, spaceForIndicators);
			Rect primarySurfaceRect = new Rect(inRect.x, indicatorsRect.yMax, inRect.width, surfaceHeight);
			Rect scrollViewBox = new Rect(inRect.x, primarySurfaceRect.yMax, inRect.width, inRect.height - indicatorsRect.height - primarySurfaceRect.height);
			Rect scrollViewOuterRect = scrollViewBox.ContractedBy(3f);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, spaceForPawnName + spaceForButtons + surfaceWidth, totalColonists * surfaceHeight);

			//Draw priority indicators
			this.DrawIndicators(indicatorsRect);

			//Primary work types
			primarySurface.OnGUI(primarySurfaceRect);

			//Draw rect edges
			DDUtilities.DrawOutline(scrollViewBox);

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

				//Pawn name
				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnName, surfaceHeight);
				this.DoPawnLabel(nameRect, pawn);

				//Disable All Work button
				Vector2 disableAllPosition = new Vector2(nameRect.xMax + spaceBetweenTypes + (DDUtilities.DraggableTextureWidth / 2f), nameRect.center.y);
				Rect disableAllRect = disableAllPosition.ToDraggableRect();
				this.ButtonDisableAll(surface, disableAllRect);

				//Reset work for pawn (enable all available types by vanilla priority order)
				Vector2 resetToVanillaPosition = new Vector2(disableAllPosition.x + spaceBetweenTypes + (DDUtilities.DraggableTextureWidth), disableAllPosition.y);
				Rect resetToVanillaRect = resetToVanillaPosition.ToDraggableRect();
				this.ButtonResetToVanilla(surface, resetToVanillaRect);

				//Surface OnGUI (check for drag, draw draggables)
				Rect surfaceRect = new Rect(nameRect.xMax + spaceForButtons, currentVerticalPosition, scrollViewInnerRect.width - nameRect.width - spaceForButtons, surfaceHeight);
				surface.OnGUI(surfaceRect);

				//Increment list y for next pawn
				currentVerticalPosition += surfaceHeight;
			}

			Widgets.EndScrollView();

			//Check for invalid drop (draggable was dropped outside of a valid surface, causes a minor bug)
			if (!Input.anyKey && Event.current.type != EventType.MouseUp && Dragger.Dragging && !scrollViewOuterRect.Contains(Dragger.CurrentDraggingObj[0].position))
			{
				Dragger.CurrentDraggingObj.Clear();
			}
		}

		private void DrawIndicators(Rect rect)
		{
			GUI.color = new Color(1f, 1f, 1f, 0.5f);

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<= " + "DD_WorkTab_HighPriorityIndicator".Translate());

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "DD_WorkTab_LowPriorityIndicator".Translate() + " =>");

			GUI.color = Color.white; // Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		//Button: set all pawn priorities to 0 (disabled)
		private void ButtonDisableAll(PawnSurface surface, Rect buttonRect)
		{
			TooltipHandler.TipRegion(buttonRect, "DD_WorkTab_ButtonDisableAll_Tooltip".Translate());

			if (Widgets.ButtonImage(buttonRect, DDUtilities.ButtonTexture_DisableAll, DDUtilities.ButtonColour, DDUtilities.HighlightColour))
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
			TooltipHandler.TipRegion(buttonRect, "DD_WorkTab_ButtonResetVanilla_Tooltip".Translate());

			if (Widgets.ButtonImage(buttonRect, DDUtilities.ButtonTexture_ResetToVanilla, DDUtilities.ButtonColour, DDUtilities.HighlightColour))
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

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(this.totalRenderWidth, this.totalRenderHeight);
			}
		}

		public override void PostClose()
		{
			base.PostClose();

			Dragger.CurrentDraggingObj.Clear();
		}
	}
}
