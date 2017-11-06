using DD_WorkTab.Base;
using DD_WorkTab.Draggables;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using DD_WorkTab.Windows;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab.Primaries
{
	public class PrimaryWork
	{
		private WorkTypeInfo workInfo;

		private bool ctrlState;

		public readonly WorkTypeDef def;

		public Rect drawRect;

		private void OnPrimaryShiftClick(int clickInt)
		{
			foreach (Pawn pawn in Find.VisibleMap.mapPawns.FreeColonists)
			{
				Controller.GetManager.GetPawnSurface(pawn).OnPrimaryShiftClick(clickInt, this.def);
			}

			if (Controller.VerboseMessages)
			{
				string shiftCompletedText = "DD_WorkTab_Message_PrimaryShiftClick".CachedTranslation(new string[] { this.def.labelShort });

				Messages.Message(shiftCompletedText, MessageTypeDefOf.SilentInput);
			}

			if (Controller.UseSounds)
			{
				Utilities.TaskCompleted.PlayOneShotOnCamera(null);
			}
		}

		private void OnPrimaryCtrlClick()
		{
			this.ctrlState = !this.ctrlState;

			foreach (Pawn pawn in Find.VisibleMap.mapPawns.FreeColonists)
			{
				PawnSurface surface = Controller.GetManager.GetPawnSurface(pawn);
				DraggableWork work = surface.childByDef[this.def];

				if (!work.CompletelyDisabled)
				{
					if (this.ctrlState)
					{
						surface.EnableWorkType(work, true);
					}

					else
					{
						surface.DisableWorkType(work, true);
					}
				}
			}

			if (Controller.VerboseMessages)
			{
				string message;

				if (this.ctrlState)
				{
					message = "DD_WorkTab_Message_PrimaryCtrlEnabledAll".CachedTranslation(new string[] { this.def.labelShort });
				}

				else
				{
					message = "DD_WorkTab_Message_PrimaryCtrlDisabledAll".CachedTranslation(new string[] { this.def.labelShort });
				}

				Messages.Message(message, MessageTypeDefOf.SilentInput);
			}

			if (Controller.UseSounds)
			{
				Utilities.TaskCompleted.PlayOneShotOnCamera(null);
			}
		}

		public void DrawTexture(Rect rect)
		{
			Utilities.DraggableOutline(rect, Utilities.MediumSkillColour);

			GUI.DrawTexture(rect.ContractedBy(2f), this.workInfo.texture64);
		}

		public void OnClicked()
		{
			if (Event.current.shift)
			{
				if (Event.current.button == 0)
				{
					this.OnPrimaryShiftClick(-1);
				}

				if (Event.current.button == 1)
				{
					this.OnPrimaryShiftClick(1);
				}
			}

			if (Event.current.control)
			{
				this.OnPrimaryCtrlClick();
			}

			Event.current.Use();
		}

		public void OnHover()
		{
			Widgets.DrawHighlight(this.drawRect);

			TooltipHandler.TipRegion(this.drawRect, Utilities.DraggableTooltip(this.def, true, false, false, false, null));
		}

		public PrimaryWork(WorkTypeDef def)
		{
			this.def = def;

			this.workInfo = Utilities.WorkDefAttributes[this.def];
		}
	}
}
