using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	[StaticConstructorOnStartup]
	public static class PrimaryTypes
	{
		private const float draggableTextureDiameter = DD_Widgets.DraggableTextureDiameter;

		private const float spaceForPawnLabel = DD_Widgets.SpaceForPawnLabel;

		private const float spaceForWorkButtons = DD_Widgets.SpaceForWorkButtons;

		private const float standardSpacing = DD_Widgets.StandardSpacing;

		private const float standardRowHeight = DD_Widgets.StandardRowHeight;

		private static readonly float standardSurfaceWidth = DD_Widgets.PawnSurfaceWidth;

		public static readonly List<DraggableWorkType> primaryDraggablesList = new List<DraggableWorkType>(); //Draggables source containing all work types

		static PrimaryTypes()
		{
			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				DraggableWorkType primeDraggable = new DraggableWorkType(def, null, true);

				primaryDraggablesList.Add(primeDraggable);
			}
		}

		public static void DoWorkTabGUI(Rect rect, Vector2 mousePosition)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			Rect compareSkillsRect = new Rect(rect.x, rect.y + standardSpacing, standardSpacing + spaceForPawnLabel, draggableTextureDiameter);
			Rect disableWorkRect = new Rect(compareSkillsRect.xMax + standardSpacing, compareSkillsRect.y, draggableTextureDiameter, draggableTextureDiameter);
			Rect resetWorkRect = new Rect(disableWorkRect.xMax + standardSpacing, disableWorkRect.y, draggableTextureDiameter, draggableTextureDiameter);

			Vector2 positionSetter = new Vector2(compareSkillsRect.xMax + spaceForWorkButtons + standardSpacing + (draggableTextureDiameter / 2f), rect.center.y);

			if (Widgets.ButtonText(compareSkillsRect, "DD_WorkTab_ButtonColonistStats".CachedTranslation(), true, false, true))
			{
				Find.WindowStack.Add(new Window_ColonistStats(DD_Settings.ColonistStatsOnlyVisibleMap));
			}

			DD_Widgets.Button(ButtonType.DisableAllWork, null, disableWorkRect, mousePosition);

			DD_Widgets.Button(ButtonType.ResetAllWork, null, resetWorkRect, mousePosition);

			foreach (DraggableWorkType primary in primaryDraggablesList)
			{
				Rect drawRect = positionSetter.ToDraggableRect();

				if (!primary.draggingNow)
				{
					primary.position = positionSetter;

					primary.dragRect = drawRect;
				}

				//Draw a copied texture in the primary type's natural position
				else
				{
					primary.DrawTexture(drawRect, false);
				}

				int clickInt = primary.DoWorkTabGUI(mousePosition);

				//Shift-click functionality to manipulate specific work type priority for all pawns
				if (clickInt != 0)
				{
					foreach (Pawn pawn in Find.VisibleMap.mapPawns.FreeColonists)
					{
						DragManager.GetPawnSurface(pawn).InsertFromPrimaryShiftClick(clickInt, primary.def);
					}
				}

				positionSetter.x += draggableTextureDiameter + standardSpacing;
			}
		}
	}
}
