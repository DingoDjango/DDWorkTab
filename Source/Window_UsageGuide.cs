using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using System.Text;

namespace DD_WorkTab
{
	public class Window_UsageGuide : Window
	{
		public const float windowWidth = 1250f;

		public const float windowHeight = 700f;

		public const float titleHeight = 36f;

		private Vector2 scrollPosition = Vector2.zero;

		private string title = "DD_WorkTab_UsageGuide_Title".Translate();

		private string tooltips = "DD_WorkTab_UsageGuide_Tooltips".Translate();

		private string colonistStats = "DD_WorkTab_UsageGuide_ColonistStats".Translate(new string[] { "DD_WorkTab_ButtonColonistStats".Translate() });

		private string colonistStatsSorting = "DD_WorkTab_UsageGuide_ColonistStats_Sorting".Translate(new string[] { "DD_WorkTab_ButtonColonistStats".Translate() });

		private string mainWorkTypes = "DD_WorkTab_UsageGuide_MainWorkTypes".Translate();

		private string pawnLabels = "DD_WorkTab_UsageGuide_PawnLabels".Translate();

		private string buttons = "DD_WorkTab_UsageGuide_Buttons".Translate(new string[]
			{
				"DD_WorkTab_ButtonDisableAll_Tooltip".Translate(),
				"DD_WorkTab_ButtonResetVanilla_Tooltip".Translate()
			});

		private string enablingWorkTypes = "DD_WorkTab_UsageGuide_EnablingWorkTypes".Translate();

		private string disablingWorkTypes = "DD_WorkTab_UsageGuide_DisablingWorkTypes".Translate();

		private string changingPriority = "DD_WorkTab_UsageGuide_ChangingPriority".Translate();

		private string windowContent
		{
			get
			{
				StringBuilder content = new StringBuilder();

				content.AppendLine(this.tooltips);
				content.AppendLine();

				content.AppendLine(this.colonistStats);
				content.AppendLine();

				content.AppendLine(this.colonistStatsSorting);
				content.AppendLine();

				content.AppendLine(this.mainWorkTypes);
				content.AppendLine();

				content.AppendLine(this.pawnLabels);
				content.AppendLine();

				content.AppendLine(this.buttons);
				content.AppendLine();

				content.AppendLine(this.enablingWorkTypes);
				content.AppendLine();

				content.AppendLine(this.disablingWorkTypes);
				content.AppendLine();

				content.Append(this.changingPriority);

				return content.ToString();
			}
		}



		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(windowWidth, windowHeight);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			this.SetInitialSizeAndPosition();
			GUI.color = Color.white;
			Text.Font = GameFont.Small;

			//Verse.Dialog_NodeTree.DoWindowContents
			//Draw title
			Rect titleRect = new Rect(inRect.x, inRect.y, inRect.width - 16f, titleHeight);
			Widgets.DrawTitleBG(titleRect);
			titleRect.xMin += 10f;
			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(titleRect, this.title);
			Text.Anchor = TextAnchor.UpperLeft; //Reset

			//List size + Rects
			Rect scrollViewOutRect = new Rect(inRect.x, titleRect.yMax + 10f, inRect.width, inRect.height - titleRect.height - 10f);
			float scrollViewWidth = scrollViewOutRect.width - 16f; //Avoid touching the scrollbar
			float scrollViewHeight = Text.CalcHeight(this.windowContent, scrollViewWidth) + 10f;
			Rect scrollViewInnerRect = new Rect(scrollViewOutRect.x, scrollViewOutRect.y, scrollViewWidth, scrollViewHeight);

			Widgets.BeginScrollView(scrollViewOutRect, ref this.scrollPosition, scrollViewInnerRect, true);

			//Draw all text
			Widgets.Label(scrollViewInnerRect, this.windowContent);

			Widgets.EndScrollView();
		}

		public Window_UsageGuide()
		{
			this.layer = WindowLayer.GameUI;
			this.doCloseX = true;
			this.closeOnClickedOutside = true;
		}
	}
}
