using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using DD_WorkTab.Windows;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Primaries
{
	public class PrimaryWork
	{
		private WorkTypeInfo workInfo;

		public readonly WorkTypeDef def;

		public Rect drawRect;

		public void DrawTexture(Rect rect)
		{
			Utilities.DraggableOutline(rect, Utilities.MediumSkillColour);

			GUI.DrawTexture(rect.ContractedBy(2f), this.workInfo.texture64);
		}

		public void OnClicked(EventData data)
		{
			if (data.shift)
			{
				int clickInt = 0;

				if (data.button == 0)
				{
					clickInt = -1;
				}

				if (data.button == 1)
				{
					clickInt = 1;
				}

				Find.WindowStack.WindowOfType<Window_WorkTab>()?.OnPrimaryShiftClick(clickInt, this.def);

				Event.current.Use();
			}
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
