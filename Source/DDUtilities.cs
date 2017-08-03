using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	[StaticConstructorOnStartup]
	public static class DDUtilities
	{
		public const float DraggableTextureWidth = 30f;

		public const float DraggableTextureHeight = 30f;

		public const float spaceForPawnLabel = 140f;

		public const float spaceForWorkButtons = 80f;

		public const float standardSpacing = 8f;

		public const float standardRowHeight = 2f * standardSpacing + DraggableTextureHeight;

		public static readonly float standardSurfaceWidth = standardSpacing + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DraggableTextureWidth + standardSpacing);

		public static readonly Dictionary<WorkTypeDef, Texture2D> WorkTypeTextures = new Dictionary<WorkTypeDef, Texture2D>();

		public static readonly Dictionary<WorkTypeDef, bool> EmergencyWorkTypes = new Dictionary<WorkTypeDef, bool>();

		public static readonly Dictionary<WorkTypeDef, string> RelevantSkillsStrings = new Dictionary<WorkTypeDef, string>();

		public static Dictionary<string, string> FastTranslationStrings = new Dictionary<string, string>();

		public static readonly Texture2D DraggableOutline = ContentFinder<Texture2D>.Get("DraggableOutline", true);

		public static readonly Texture2D ButtonTexture_DisableAll = ContentFinder<Texture2D>.Get("ButtonDisableAll", true);

		public static readonly Texture2D ButtonTexture_ResetToVanilla = ContentFinder<Texture2D>.Get("ButtonResetToDefaults", true);

		public static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true);

		public static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true);

		public static readonly Texture2D HaltIcon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true); //Temporary from vanilla

		public static readonly Color OutlineColour = new Color(0.6f, 0.6f, 0.6f, 1f); //Light grey

		public static readonly Color ListSeparatorColour = new Color(0.3f, 0.3f, 0.3f, 1f); //Faded grey

		public static readonly Color IndicatorsColour = new Color(1f, 1f, 1f, 0.5f); //Transparent grey

		public static readonly Color ButtonColour = new Color(1f, 0.65f, 0f); //Orange

		public static readonly Color LowSkillColour = new Color(0.5f, 0.5f, 0.5f); //Dark grey, also used for no-skill work types

		public static readonly Color MediumSkillColour = Color.white;

		public static readonly Color HighSkillColour = new Color(0f, 0.6f, 1f); //Dark blue

		public static readonly Color VeryHighSkillColour = Color.green;

		public static readonly Color ExcellentSkillColour = new Color(1f, 0.85f, 0f); /* GOLD http://whenisnlss.com/assets/sounds/GOLD.mp3 */

		static DDUtilities()
		{
			foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs)
			{
				WorkTypeTextures[def] = ContentFinder<Texture2D>.Get("WorkTypeIcons/" + def.defName);

				EmergencyWorkTypes[def] = def.workGiversByPriority.Any(wg => wg.emergency);

				if (def.relevantSkills.Count > 0)
				{
					string relevantSkills = string.Empty;

					foreach (SkillDef skill in def.relevantSkills)
					{
						relevantSkills += skill.skillLabel + ", ";
					}

					RelevantSkillsStrings[def] = relevantSkills.Substring(0, relevantSkills.Length - 2);
				}
			}
		}

		//Provides a square whose center point is the provided position
		public static Rect ToDraggableRect(this Vector2 position)
		{
			float halfWidth = DraggableTextureWidth / 2f;
			float halfHeight = DraggableTextureHeight / 2f;

			return new Rect(position.x - halfWidth, position.y - halfHeight, DraggableTextureWidth, DraggableTextureHeight);
		}

		//Enum iteration utilities
		public static SortOrder Next(this SortOrder order)
		{
			switch (order)
			{
				case SortOrder.Undefined:
					return SortOrder.Descending;
				case SortOrder.Descending:
					return SortOrder.Ascending;
				case SortOrder.Ascending:
					return SortOrder.Undefined;
				default:
					return SortOrder.Undefined;
			}
		}

		public static SortOrder Previous(this SortOrder order)
		{
			switch (order)
			{
				case SortOrder.Undefined:
					return SortOrder.Ascending;
				case SortOrder.Descending:
					return SortOrder.Undefined;
				case SortOrder.Ascending:
					return SortOrder.Descending;
				default:
					return SortOrder.Undefined;
			}
		}

		public static void DrawPriorityIndicators(Rect rect)
		{
			GUI.color = IndicatorsColour;

			Text.Anchor = TextAnchor.MiddleLeft;
			Widgets.Label(rect, "<= " + "HigherPriority".TranslateFast());

			Text.Anchor = TextAnchor.MiddleRight;
			Widgets.Label(rect, "LowerPriority".TranslateFast() + " =>");

			GUI.color = Color.white; // Reset
			Text.Anchor = TextAnchor.UpperLeft; //Reset
		}

		public static string GetLabelForPawn(Pawn pawn)
		{
			//RimWorld.PawnColumnWorker_Label.DoCell
			string nameAdjusted;

			if (!pawn.RaceProps.Humanlike && pawn.Name != null && !pawn.Name.Numerical)
			{
				nameAdjusted = pawn.Name.ToStringShort.CapitalizeFirst() + ", " + pawn.KindLabel;
			}

			else
			{
				nameAdjusted = pawn.LabelCap;
			}

			return nameAdjusted;
		}

		//Draw rectangular outline on Rect edges
		public static void DrawOutline(Rect rect, bool isEmergency = false, bool scrollBox = false)
		{
			GUI.color = !isEmergency ? OutlineColour : Color.red;

			if (!scrollBox)
			{
				GUI.DrawTexture(rect, DraggableOutline);
			}

			else
			{
				GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), BaseContent.WhiteTex); //Ceiling
				GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Left wall
				GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Right wall
				GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - 1f, rect.width, 1f), BaseContent.WhiteTex); //Floor
			}

			GUI.color = Color.white; //Reset
		}

		//Draw light-grey horizontal line
		public static void DrawListSeparator(Rect rect, float verticalPos)
		{
			GUI.color = ListSeparatorColour;
			Rect position = new Rect(rect.x, verticalPos, rect.width, 1f);

			GUI.DrawTexture(position, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		public static void DoPawnLabel(Rect rect, Pawn pawn)
		{
			GUI.color = Color.white;
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;

			Widgets.Label(rect, GetLabelForPawn(pawn));

			Text.Anchor = TextAnchor.UpperLeft; //Reset
			Text.WordWrap = true; //Reset

			if (Widgets.ButtonInvisible(rect, false))
			{
				CameraJumper.TryJumpAndSelect(pawn);

				//Close open DD windows (vanilla does not close on jump-select, but it seems natural)
				Find.WindowStack.TryRemove(typeof(Window_ColonistStats), false);
				Find.WindowStack.TryRemove(typeof(Window_WorkTab), false);
			}

			if (Mouse.IsOver(rect))
			{
				Widgets.DrawHighlight(rect);

				TooltipHandler.TipRegion(rect, "ClickToJumpTo".TranslateFast() + "\n\n" + pawn.GetTooltip().text);
			}
		}

		public static string GetDraggableTooltip(WorkTypeDef def, bool statsWindow, bool primary, Pawn worker)
		{
			StringBuilder tooltip = new StringBuilder(def.labelShort);

			if (EmergencyWorkTypes[def])
			{
				tooltip.Append("DD_WorkTab_WorkTypeHasEmergency".TranslateFast());
			}

			if (primary)
			{
				tooltip.Append("\n\n");
				tooltip.Append(def.description);

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_PrimeDraggable_DragTip".TranslateFast(new string[] { def.gerundLabel }));
				}

				else
				{
					tooltip.Append("\n\n");
					tooltip.Append("ClickToSortByThisColumn".TranslateFast());
				}
			}

			//RimWorld.WidgetsWork.TipForPawnWorker
			else
			{
				if (def.relevantSkills.Count > 0)
				{
					tooltip.Append("\n\n");
					tooltip.Append("RelevantSkills".TranslateFast(new object[]
					{
						RelevantSkillsStrings[def],
						worker.skills.AverageOfRelevantSkillsFor(def).ToString(),
						SkillRecord.MaxLevel
					}));
				}

				if (IsIncapableOfWholeWorkType(worker, def))
				{
					tooltip.Append("\n\n");
					tooltip.Append("IncapableOfWorkTypeBecauseOfCapacities".TranslateFast());
				}

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_PawnDraggable_DragTip".TranslateFast());
				}
			}

			return tooltip.ToString();
		}

		public static void Button_DisableAllWork(bool forAllPawnsOnMap, PawnSurface surface, Rect buttonRect)
		{
			DrawOutline(buttonRect, false, false);

			string tooltip;
			string text;
			string title;
			Action buttonAction;

			if (forAllPawnsOnMap)
			{
				tooltip = "DD_WorkTab_ButtonDisableAllVisibleMap_Tooltip".TranslateFast();
				text = "DD_WorkTab_ButtonDisableAllVisibleMap_Text".TranslateFast();
				title = "DD_WorkTab_ButtonDisableAllVisibleMap_Title".TranslateFast();
				buttonAction = DisableAllWorkForVisibleMap;
			}

			else
			{
				tooltip = "DD_WorkTab_ButtonDisableAll_Tooltip".TranslateFast();
				text = "DD_WorkTab_ButtonDisableAll_Text".TranslateFast().AdjustedFor(surface.pawn);
				title = "DD_WorkTab_ButtonDisableAll_Title".TranslateFast().AdjustedFor(surface.pawn);
				buttonAction = surface.DisableAllWork;
			}

			GUI.color = ButtonColour;
			GUI.DrawTexture(buttonRect.ContractedBy(2f), ButtonTexture_DisableAll);
			GUI.color = Color.white; //Reset

			if (Widgets.ButtonInvisible(buttonRect, false))
			{
				if (Settings.ShowPrompt)
				{
					DiaOption acceptButton = new DiaOption("DD_WorkTab_ButtonOption_Accept".TranslateFast());
					acceptButton.action = buttonAction;
					acceptButton.resolveTree = true;

					DiaOption rejectButton = new DiaOption("DD_WorkTab_ButtonOption_Cancel".TranslateFast());
					rejectButton.resolveTree = true;

					DiaOption acceptDoNotShowAgain = new DiaOption("DD_WorkTab_ButtonOption_AcceptDisablePrompt".TranslateFast());
					acceptDoNotShowAgain.action = delegate
					{
						Settings.ShowPrompt = false;

						buttonAction();
					};
					acceptDoNotShowAgain.resolveTree = true;

					DiaNode prompt = new DiaNode(text + "DD_WorkTab_ButtonText_DisablePrompt".TranslateFast());
					prompt.options.Add(acceptButton);
					prompt.options.Add(rejectButton);
					prompt.options.Add(acceptDoNotShowAgain);

					Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, title));
				}

				else
				{
					buttonAction();
				}
			}

			if (Mouse.IsOver(buttonRect))
			{
				Widgets.DrawHighlight(buttonRect);

				TooltipHandler.TipRegion(buttonRect, tooltip);
			}
		}

		public static void Button_ResetWorkToVanilla(bool forAllPawnsOnMap, PawnSurface surface, Rect buttonRect)
		{
			DrawOutline(buttonRect, false, false);

			string tooltip;
			string text;
			string title;
			Action buttonAction;

			if (forAllPawnsOnMap)
			{
				tooltip = "DD_WorkTab_ButtonResetAllToVanilla_Tooltip".TranslateFast();
				text = "DD_WorkTab_ButtonResetAllToVanilla_Text".TranslateFast();
				title = "DD_WorkTab_ButtonResetAllToVanilla_Title".TranslateFast();
				buttonAction = ResetAllWorkForVisibleMap;
			}

			else
			{
				tooltip = "DD_WorkTab_ButtonResetVanilla_Tooltip".TranslateFast();
				text = "DD_WorkTab_ButtonResetVanilla_Text".TranslateFast().AdjustedFor(surface.pawn);
				title = "DD_WorkTab_ButtonResetVanilla_Title".TranslateFast().AdjustedFor(surface.pawn);
				buttonAction = surface.ResetToVanillaSettings;
			}

			GUI.color = ButtonColour;
			GUI.DrawTexture(buttonRect.ContractedBy(2f), ButtonTexture_ResetToVanilla);
			GUI.color = Color.white; //Reset

			if (Widgets.ButtonInvisible(buttonRect, false))
			{
				if (Settings.ShowPrompt)
				{
					DiaOption acceptButton = new DiaOption("DD_WorkTab_ButtonOption_Accept".TranslateFast());
					acceptButton.action = buttonAction;
					acceptButton.resolveTree = true;

					DiaOption rejectButton = new DiaOption("DD_WorkTab_ButtonOption_Cancel".TranslateFast());
					rejectButton.resolveTree = true;

					DiaOption acceptDoNotShowAgain = new DiaOption("DD_WorkTab_ButtonOption_AcceptDisablePrompt".TranslateFast());
					acceptDoNotShowAgain.action = delegate
					{
						Settings.ShowPrompt = false;

						buttonAction();
					};
					acceptDoNotShowAgain.resolveTree = true;

					DiaNode prompt = new DiaNode(text + "DD_WorkTab_ButtonText_DisablePrompt".TranslateFast());
					prompt.options.Add(acceptButton);
					prompt.options.Add(rejectButton);
					prompt.options.Add(acceptDoNotShowAgain);

					Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, title));
				}

				else
				{
					buttonAction();
				}
			}

			if (Mouse.IsOver(buttonRect))
			{
				Widgets.DrawHighlight(buttonRect);

				TooltipHandler.TipRegion(buttonRect, tooltip);
			}
		}

		//RimWorld.PawnColumnWorker_WorkPriority (invoked many times per frame, so copied instead of reflected)
		private static bool IsIncapableOfWholeWorkType(Pawn p, WorkTypeDef work)
		{
			for (int i = 0; i < work.workGiversByPriority.Count; i++)
			{
				bool flag = true;
				for (int j = 0; j < work.workGiversByPriority[i].requiredCapacities.Count; j++)
				{
					PawnCapacityDef capacity = work.workGiversByPriority[i].requiredCapacities[j];
					if (!p.health.capacities.CapableOf(capacity))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return false;
				}
			}
			return true;
		}

		private static void DisableAllWorkForVisibleMap()
		{
			foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
			{
				Dragger.GetPawnSurface(p).DisableAllWork();
			}
		}

		private static void ResetAllWorkForVisibleMap()
		{
			foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
			{
				Dragger.GetPawnSurface(p).ResetToVanillaSettings();
			}
		}

		public static string TranslateFast(this string text, object[] args = null)
		{
			string finalString;

			if (!FastTranslationStrings.TryGetValue(text, out finalString))
			{
				finalString = text.Translate();

				FastTranslationStrings[text] = finalString;
			}

			if (args != null)
			{
				return string.Format(finalString, args);
			}

			return finalString;
		}
	}
}
