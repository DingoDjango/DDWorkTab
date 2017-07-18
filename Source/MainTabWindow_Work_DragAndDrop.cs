﻿using System;
using System.Collections.Generic;
using System.Reflection;
//using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class MainTabWindow_Work_DragAndDrop : MainTabWindow
	{
		private DragHelper dragger = Current.Game.GetComponent<DragHelper>();

		private WorkTypeSurface surfaceForPrimaryTypes = new WorkTypeSurface();

		private static float wTypeTexWidth = DDUtilities.WorkTypeTextureSize.x;

		private static float wTypeTexHeight = DDUtilities.WorkTypeTextureSize.y;

		private const float spaceForPawnName = 150f;

		private const float spaceForButtons = 70f;

		private const float listItemHeight = 50f;

		internal Vector2 scrollPosition = Vector2.zero;

		private int workTypesCount
		{
			get
			{
				int numberOfWorkTypes = 0;

				foreach (var workType in DefDatabase<WorkTypeDef>.AllDefs)
				{
					numberOfWorkTypes++;
				}

				return numberOfWorkTypes;
			}
		}

		private float widthNecessaryForAllTextures
		{
			get
			{
				return 10f + this.workTypesCount * (wTypeTexWidth + 10f); //10f initial padding + width per texture and additional 10f spacing
			}
		}

		private int totalColonists
		{
			get
			{
				int colonistCount = 0;

				foreach (var pawn in Find.VisibleMap.mapPawns.FreeColonists)
				{
					colonistCount++;
				}

				return colonistCount;
			}
		}

		private float totalRenderWidth
		{
			get
			{
				return spaceForPawnName + spaceForButtons + this.widthNecessaryForAllTextures + 30f;
			}
		}

		private float totalRenderHeight
		{
			get
			{
				return listItemHeight * this.totalColonists + listItemHeight + 30f; //Accounted for mainTypesRect
			}
		}

		public MainTabWindow_Work_DragAndDrop()
		{
			//Populate the main surface with indices (only needs to be done once realistically)
			int currentMainTypePriority = 1;
			foreach (var typeDef in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				DraggableWorkType primeDraggable = new DraggableWorkType(this.surfaceForPrimaryTypes, typeDef, currentMainTypePriority);
				primeDraggable.isPrimaryType = true;
				surfaceForPrimaryTypes.AddOrUpdateChild(primeDraggable);

				currentMainTypePriority++;
			}

			Current.Game.playSettings.useWorkPriorities = true;
		}

		public override Vector2 RequestedTabSize
		{
			get
			{
				return new Vector2(this.totalRenderWidth + 30f, this.totalRenderHeight + 30f);
			}
		}

		public override void PreOpen()
		{
			base.PreOpen();

			Current.Game.playSettings.useWorkPriorities = true; //Just in case?

			this.scrollPosition = Vector2.zero; //Temporary for testing, might be necessary

			foreach (var p in Find.VisibleMap.mapPawns.FreeColonists)
			{
				DDUtilities.RefreshPawnPriorities(p);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			base.DoWindowContents(inRect);
			GUI.color = Color.white;

			Rect mainTypesRect = new Rect(inRect.x + spaceForPawnName + spaceForButtons, inRect.y, inRect.width, listItemHeight);

			#region MainWorkTypes
			Vector2 mainTypesPositionSetter = new Vector2(mainTypesRect.x + 10f + (wTypeTexWidth / 2f), mainTypesRect.center.y);

			foreach (var mainWorkType in this.surfaceForPrimaryTypes.childrenSortedByPriority)
			{
				if (!mainWorkType.IsDragging)
				{
					mainWorkType.position = mainTypesPositionSetter;
				}

				//Static texture setter (always draw main work types even if dragging)
				Vector2 primaryPosition = mainTypesPositionSetter;
				Rect drawRect = DDUtilities.RectOnVector(primaryPosition, DDUtilities.WorkTypeTextureSize);
				GUI.DrawTexture(drawRect, DDUtilities.TextureFromModFolder(mainWorkType.def));
				Widgets.DrawHighlightIfMouseover(drawRect);
				TooltipHandler.TipRegion(drawRect, mainWorkType.def.labelShort);

				//Standard OnGUI including drag controls
				mainWorkType.OnGUI();

				mainTypesPositionSetter.x += wTypeTexWidth + 10f;
			}
			#endregion

			#region ScrollingList
			//Scrolling list - outer Rect
			Rect outRect = new Rect(inRect.x, inRect.y + mainTypesRect.height, inRect.width, inRect.height - mainTypesRect.height);
			//Scrolling list - virtual Rect
			Rect viewRect = new Rect(inRect.x, inRect.y + mainTypesRect.height, totalRenderWidth, totalColonists * listItemHeight);

			Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);

			//Render all pawns and their surfaces
			float currentVerticalPosition = viewRect.y;
			foreach (var pawn in Find.VisibleMap.mapPawns.FreeColonists)
			{
				var pawnSurface = dragger.SurfaceForPawn(pawn);

				#region ColonistName
				Rect nameRect = new Rect(viewRect.x, currentVerticalPosition, spaceForPawnName, listItemHeight);

				string pawnLabel = DDUtilities.GetLabelForPawn(pawn);

				Text.Font = GameFont.Small;
				Text.Anchor = TextAnchor.MiddleLeft;
				Text.WordWrap = false;
				Widgets.Label(nameRect, pawnLabel);

				Text.Anchor = TextAnchor.UpperLeft; //Reset
				Text.WordWrap = true; //Reset
				#endregion

				//Buttons area
				Rect buttonsRect = new Rect(nameRect.xMax, currentVerticalPosition, spaceForButtons, listItemHeight);

				//Disable all workTypes button
				Vector2 disableAllVector = new Vector2(buttonsRect.xMin + (wTypeTexWidth / 2f), buttonsRect.center.y);
				Rect disableAllRect = DDUtilities.RectOnVector(disableAllVector, DDUtilities.WorkTypeTextureSize);
				Texture2D disableAllTexture = ContentFinder<Texture2D>.Get("ButtonDisableAll");
				GUI.DrawTexture(disableAllRect, disableAllTexture);
				Widgets.DrawHighlightIfMouseover(disableAllRect);
				TooltipHandler.TipRegion(disableAllRect, "Disable all work"); //Todo: translate
				if (Widgets.ButtonInvisible(disableAllRect))
				{
					//Todo: add prompt
					pawnSurface.DisableAllWork();
				}

				//Reset work priorities according to vanilla order
				Vector2 resetToVanillVector = new Vector2(buttonsRect.xMax - (wTypeTexWidth / 2f), buttonsRect.center.y);
				Rect resetToVanillaRect = DDUtilities.RectOnVector(resetToVanillVector, DDUtilities.WorkTypeTextureSize);
				Texture2D resetToVanillaTexture = ContentFinder<Texture2D>.Get("ButtonResetToDefaults");
				GUI.DrawTexture(resetToVanillaRect, resetToVanillaTexture);
				Widgets.DrawHighlightIfMouseover(resetToVanillaRect);
				TooltipHandler.TipRegion(resetToVanillaRect, "Reset to vanilla priorities"); //Todo: translate
				if (Widgets.ButtonInvisible(resetToVanillaRect))
				{
					//Todo: add prompt
					pawnSurface.ResetToVanillaSettings();
				}

				#region WorkAssignmentArea
				Rect surfaceRect = new Rect(buttonsRect.xMax, currentVerticalPosition, viewRect.width - nameRect.width - buttonsRect.width, listItemHeight);

				pawnSurface.currentListRect = surfaceRect;
				pawnSurface.OnGUI();
				#endregion

				currentVerticalPosition += listItemHeight; //Increment list y for next pawn
			}

			Widgets.EndScrollView();
			#endregion
		}
	}
}
