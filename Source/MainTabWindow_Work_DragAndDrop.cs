using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class MainTabWindow_Work_DragAndDrop : MainTabWindow
	{
		public const float spaceForPawnName = 140f;

		public const float spaceForButtons = 80f;

		public const float spaceBetweenTypes = 5f;

		private PrimarySurface primarySurface = new PrimarySurface();

		private float surfaceHeight = 2f * spaceBetweenTypes + DDUtilities.DraggableTextureHeight;

		private float surfaceWidth = spaceBetweenTypes + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DDUtilities.DraggableTextureWidth + spaceBetweenTypes);

		private Vector2 scrollPosition = Vector2.zero;

		private DragManager dragger
		{
			get
			{
				return Current.Game.World.GetComponent<DragManager>();
			}
		}

		public MainTabWindow_Work_DragAndDrop()
		{
			Current.Game.playSettings.useWorkPriorities = true;
		}

		private int totalColonists
		{
			get
			{
				return Find.VisibleMap.mapPawns.FreeColonistsCount;
			}
		}

		private float totalRenderWidth
		{
			get
			{
				return spaceForPawnName + spaceForButtons + this.surfaceWidth + 60f;
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

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(this.totalRenderWidth, this.totalRenderHeight);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);

			GUI.color = new Color(1f, 1f, 1f, 0.5f);
			Text.Font = GameFont.Tiny;
			float spaceForIndicators = Text.LineHeight;

			//General Rects
			Rect indicatorsRect = new Rect(inRect.x + spaceForPawnName + spaceForButtons, inRect.y, inRect.width - spaceForPawnName - spaceForButtons, spaceForIndicators);
			Rect primarySurfaceRect = new Rect(inRect.x, indicatorsRect.yMax, inRect.width, surfaceHeight);
			Rect scrollViewOuterRect = new Rect(inRect.x, primarySurfaceRect.yMax, inRect.width, inRect.height - indicatorsRect.height - primarySurfaceRect.height);
			Rect scrollViewInnerRect = new Rect(scrollViewOuterRect.x, scrollViewOuterRect.y, spaceForPawnName + spaceForButtons + this.surfaceWidth, totalColonists * surfaceHeight);

			//Draw priority indicators
			this.DrawIndicators(indicatorsRect);

			//Primary work types
			primarySurface.OnGUI(primarySurfaceRect); /* <-- add buttons / text / help related stuff here */

			Widgets.BeginScrollView(scrollViewOuterRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//All pawns and pawn surfaces
			float currentVerticalPosition = scrollViewInnerRect.yMin;

			foreach (Pawn pawn in Find.VisibleMap.mapPawns.FreeColonists)
			{
				//List separator
				GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
				Widgets.DrawLineHorizontal(inRect.x, currentVerticalPosition, inRect.width);
				GUI.color = Color.white; //Reset

				PawnSurface surface = dragger.SurfaceForPawn(pawn);

				//Pawn name
				Rect nameRect = new Rect(scrollViewInnerRect.x, currentVerticalPosition, spaceForPawnName, surfaceHeight);
				this.DrawPawnLabel(nameRect, pawn);

				//Disable All Work button
				Vector2 disableAllPosition = new Vector2(nameRect.xMax + spaceBetweenTypes + (DDUtilities.DraggableTextureWidth / 2f), nameRect.center.y);
				Rect disableAllRect = DDUtilities.RectOnVector(disableAllPosition, DDUtilities.DraggableSize);
				this.ButtonDisableAll(surface, disableAllRect);

				//Reset work for pawn (enable all available types by vanilla priority order)
				Vector2 resetToVanillaPosition = new Vector2(disableAllPosition.x + spaceBetweenTypes + (DDUtilities.DraggableTextureWidth), disableAllPosition.y);
				Rect resetToVanillaRect = DDUtilities.RectOnVector(resetToVanillaPosition, DDUtilities.DraggableSize);
				this.ButtonResetToVanilla(surface, resetToVanillaRect);

				//Surface OnGUI (check for drag, draw draggables)
				Rect surfaceRect = new Rect(nameRect.xMax + spaceForButtons, currentVerticalPosition, scrollViewInnerRect.width - nameRect.width - spaceForButtons, surfaceHeight);
				surface.OnGUI(surfaceRect);

				//Increment list y for next pawn
				currentVerticalPosition += surfaceHeight;
			}

			Widgets.EndScrollView();
		}

		public override void PostClose()
		{
			base.PostClose();

			dragger.CurrentDraggingObj.Clear();
		}

		private void DrawIndicators(Rect rect)
		{
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<=" + " higher priority");

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "lower priority " + "=>");

			GUI.color = Color.white; // Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		private void DrawPawnLabel(Rect rect, Pawn pawn)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;

			Widgets.Label(rect, DDUtilities.GetLabelForPawn(pawn));

			Text.Anchor = TextAnchor.UpperLeft; //Reset
			Text.WordWrap = true; //Reset
		}

		//Button: set all pawn priorities to 0 (disabled)
		private void ButtonDisableAll(PawnSurface surface, Rect buttonRect)
		{
			GUI.DrawTexture(buttonRect, DDUtilities.ButtonTexture_DisableAll);

			Widgets.DrawHighlightIfMouseover(buttonRect);

			TooltipHandler.TipRegion(buttonRect, "DISABLE_ALL_WORK"); //Todo: translate

			if (Widgets.ButtonInvisible(buttonRect))
			{
				if (Settings.ShowPrompt)
				{
					DiaOption acceptButton = new DiaOption("Accept");
					acceptButton.action = delegate
					{
						surface.DisableAllWork();
					};
					acceptButton.resolveTree = true;

					DiaOption rejectButton = new DiaOption("Cancel");
					rejectButton.resolveTree = true;

					DiaOption acceptDoNotShowAgain = new DiaOption("Accept '(do not show this again)'");
					acceptDoNotShowAgain.action = delegate
					{
						Settings.ShowPrompt = false;

						surface.DisableAllWork();
					};
					acceptButton.resolveTree = true;

					DiaNode prompt = new DiaNode("This action will set all of the colonist's priorities to 0.\n\nChoose 'Cancel' if you clicked the button by mistake.\n\nIf you choose 'do not show again' you may re-enable this prompt from Options > Mod Settings.");
					prompt.options.Add(acceptButton);
					prompt.options.Add(rejectButton);
					prompt.options.Add(acceptDoNotShowAgain);

					Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, "WARNING_BUTTON_DISABLEALL"));
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
			GUI.DrawTexture(buttonRect, DDUtilities.ButtonTexture_ResetToVanilla);

			Widgets.DrawHighlightIfMouseover(buttonRect);

			TooltipHandler.TipRegion(buttonRect, "RESET_TO_VANILLA"); //Todo: translate

			if (Widgets.ButtonInvisible(buttonRect))
			{
				if (Settings.ShowPrompt)
				{
					DiaOption acceptButton = new DiaOption("Accept");
					acceptButton.action = delegate
					{
						surface.ResetToVanillaSettings();
					};
					acceptButton.resolveTree = true;

					DiaOption rejectButton = new DiaOption("Cancel");
					rejectButton.resolveTree = true;

					DiaOption acceptDoNotShowAgain = new DiaOption("Accept '(do not show this again)'");
					acceptDoNotShowAgain.action = delegate
					{
						Settings.ShowPrompt = false;

						surface.ResetToVanillaSettings();
					};
					acceptButton.resolveTree = true;

					DiaNode prompt = new DiaNode("This action will enable all possible work types for this colonist by their natural priority.\n\nChoose 'Cancel' if you clicked the button by mistake.\n\nIf you choose 'do not show again' you may re-enable this prompt from Options > Mod Settings.");
					prompt.options.Add(acceptButton);
					prompt.options.Add(rejectButton);
					prompt.options.Add(acceptDoNotShowAgain);

					Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, "WARNING_BUTTON_RESETTOVANILLA"));
				}

				else
				{
					surface.ResetToVanillaSettings();
				}
			}
		}
	}
}
