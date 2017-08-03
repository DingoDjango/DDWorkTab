using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	//This surface is responsible for drawing primary DraggableWorkTypes and general buttons at the top of the window
	public class PrimarySurface
	{
		private const float standardSpacing = DDUtilities.standardSpacing;

		private const float spaceForPawnLabel = DDUtilities.spaceForPawnLabel;

		private const float spaceForWorkButtons = DDUtilities.spaceForWorkButtons;

		private const float draggableWidth = DDUtilities.DraggableTextureWidth;

		private const float draggableHeight = DDUtilities.DraggableTextureHeight;

		private static float surfaceWidth = DDUtilities.standardSurfaceWidth;

		private List<DraggableWorkType> primeDraggables = new List<DraggableWorkType>();

		public List<DraggableWorkType> PrimeDraggablesList
		{
			get
			{
				return this.primeDraggables;
			}
		}

		public void OnWorkTabGUI(Rect rect)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			//Compare Skills button
			Rect compareSkillsRect = new Rect(rect.x, rect.y + standardSpacing, standardSpacing + spaceForPawnLabel, draggableHeight);

			if (Widgets.ButtonText(compareSkillsRect, "DD_WorkTab_ButtonColonistStats".TranslateFast(), true, false, true))
			{
				Find.WindowStack.Add(new Window_ColonistStats(Settings.ColonistStatsOnlyVisibleMap));
			}

			//Disable All (for everyone) button
			Rect disableWorkRect = new Rect(compareSkillsRect.xMax + standardSpacing, compareSkillsRect.y, draggableWidth, draggableHeight);

			DDUtilities.Button_DisableAllWork(true, null, disableWorkRect);

			//Reset All Work (for everyone) button
			Rect resetWorkRect = new Rect(disableWorkRect.xMax + standardSpacing, disableWorkRect.y, draggableWidth, draggableHeight);

			DDUtilities.Button_ResetWorkToVanilla(true, null, resetWorkRect);

			//Primary work types
			Vector2 positionSetter = new Vector2(compareSkillsRect.xMax + spaceForWorkButtons + standardSpacing + (draggableWidth / 2f), rect.center.y);

			for (int i = 0; i < this.primeDraggables.Count; i++)
			{
				DraggableWorkType draggable = this.primeDraggables[i];

				Rect drawRect = positionSetter.ToDraggableRect();

				if (!draggable.IsDragging)
				{
					draggable.position = positionSetter;
				}

				else
				{
					draggable.DrawTexture(drawRect);
				}

				draggable.OnWorkTabGUI();

				positionSetter.x += draggableWidth + standardSpacing;
			}
		}

		//Draggables source containing all work types
		public PrimarySurface()
		{
			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				DraggableWorkType primeDraggable = new DraggableWorkType(def, null, true);

				this.primeDraggables.Add(primeDraggable);
			}
		}
	}
}
