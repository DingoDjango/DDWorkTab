using System;
using System.Collections.Generic;
using System.Text;
using DD_WorkTab.Base;
using DD_WorkTab.Draggables;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Primaries;
using DD_WorkTab.Windows;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab.Tools
{
	[StaticConstructorOnStartup]
	public static class Utilities
	{
		public const float DraggableTextureDiameter = 48f;

		public const float PassionDrawSize = 14f;

		public const float ShortSpacing = 8f;

		public const float SpaceForPawnLabel = 140f;

		public const float SpaceForScrollBar = 20f;

		public const float SpaceForWorkButtons = 2f * DraggableTextureDiameter + 3f * ShortSpacing;

		public const float StandardRowHeight = 2f * ShortSpacing + DraggableTextureDiameter;

		public static readonly float PawnSurfaceWidth = 2f * ShortSpacing + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DraggableTextureDiameter + ShortSpacing);

		public static readonly float TinyTextLineHeight;

		public static readonly Texture2D DraggableOutlineTexture = DingoUtils.GetHQTexture("DraggableOutline");

		public static readonly Texture2D DisableWorkTexture = DingoUtils.GetHQTexture("DisableWork");

		public static readonly Texture2D ResetWorkTexture = DingoUtils.GetHQTexture("ResetWork");

		public static readonly Texture2D SortingDescendingIcon = DingoUtils.GetHQTexture("SortingDescending");

		public static readonly Texture2D SortingAscendingIcon = DingoUtils.GetHQTexture("Sorting");

		public static readonly Texture2D PassionMinorIcon = DingoUtils.GetHQTexture("PassionMinor");

		public static readonly Texture2D PassionMajorIcon = DingoUtils.GetHQTexture("PassionMajor");

		public static readonly Texture2D IncapableWorkerX = DingoUtils.GetHQTexture("IncapableWorkerX");

		public static readonly Color OutlineColour = new Color(0.6f, 0.6f, 0.6f); //Light grey

		public static readonly Color ListSeparatorColour = new Color(0.3f, 0.3f, 0.3f); //Faded grey

		public static readonly Color IndicatorsColour = new Color(1f, 1f, 1f, 0.5f); //Transparent grey

		public static readonly Color LowSkillColour = new Color(0.8f, 0.8f, 0.8f, 0.5f); //Grey

		public static readonly Color MediumSkillColour = Color.white;

		public static readonly Color HighSkillColour = new Color(0f, 0.6f, 1f); //Dark blue

		public static readonly Color VeryHighSkillColour = Color.green;

		public static readonly Color ExcellentSkillColour = new Color(1f, 0.85f, 0f); /* GOLD http://whenisnlss.com/assets/sounds/GOLD.mp3 */

		public static readonly SoundDef TaskCompleted = MessageTypeDefOf.PositiveEvent.sound;

		public static readonly SoundDef TaskFailed = SoundDefOf.ClickReject;

		public static readonly SoundDef WorkEnabled = SoundDefOf.LessonActivated;

		public static readonly SoundDef WorkDisabled = SoundDefOf.LessonDeactivated;

		public static readonly Dictionary<WorkTypeDef, WorkTypeInfo> WorkDefAttributes = new Dictionary<WorkTypeDef, WorkTypeInfo>();

		public static float MaxWindowWidth => (float)UI.screenWidth * 0.8f;

		public static float MaxWindowHeight => (float)UI.screenHeight * 0.8f;

		//DingoUtils.CachedStrings[string] => cached translation
		//DingoUtils.CachedStrings[WorkTypeDef] => relevant skills for def
		//DingoUtils.CachedStrings[Pawn] => pawn label

		static Utilities()
		{
			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				HashSet<PawnCapacityDef> allRequiredCapacities = new HashSet<PawnCapacityDef>();

				for (int i = 0; i < def.workGiversByPriority.Count; i++)
				{
					WorkGiverDef workGiver = def.workGiversByPriority[i];

					for (int j = 0; j < workGiver.requiredCapacities.Count; j++)
					{
						allRequiredCapacities.Add(workGiver.requiredCapacities[j]);
					}
				}

				Texture2D enabledTex = DingoUtils.GetHQTexture(def.defName, "Work");
				Texture2D disabledTex = DingoUtils.GetHQTexture(def.defName, "Work_Disabled");
				Texture2D greyscaleTex = DingoUtils.GetHQTexture(def.defName, "Work_Greyscale");

				WorkDefAttributes[def] = new WorkTypeInfo(enabledTex, disabledTex, greyscaleTex, allRequiredCapacities);

				//Cache "relevant skills:" string
				if (def.relevantSkills.Count > 0)
				{
					string relevantSkills = string.Empty;

					for (int k = 0; k < def.relevantSkills.Count; k++)
					{
						relevantSkills += def.relevantSkills[k].skillLabel + ", ";
					}

					DingoUtils.CachedStrings[def] = relevantSkills.Substring(0, relevantSkills.Length - 2);
				}
			}

			//Modify vanilla translations for better tooltip building
			DingoUtils.CachedStrings["ClickToSortByThisColumn"] = "\n\n" + "ClickToSortByThisColumn".Translate();
			DingoUtils.CachedStrings["RelevantSkills"] = "\n\n" + "RelevantSkills".Translate();
			DingoUtils.CachedStrings["ClickToJumpTo"] = "ClickToJumpTo".Translate() + "\n\n";

			Text.Font = GameFont.Tiny;
			TinyTextLineHeight = Text.LineHeight;
			Text.Font = GameFont.Small; //Reset

			Controller.GetPrimaries = new PrimarySurface();
		}

		/// <summary>
		/// Provides a Draggable-sized square whose center point is the provided position.
		/// </summary>
		public static Rect ToDraggableRect(this Vector2 position, float diameter = DraggableTextureDiameter)
		{
			float draggableRadius = diameter / 2f;

			return new Rect(position.x - draggableRadius, position.y - draggableRadius, diameter, diameter);
		}

		/// <summary>
		/// Quickly fetches a pawn's cached name string.
		/// </summary>
		public static string CachedPawnLabel(this Pawn pawn)
		{
			if (!DingoUtils.CachedStrings.TryGetValue(pawn, out string pawnLabel))
			{
				//RimWorld.PawnColumnWorker_Label.DoCell
				if (!pawn.RaceProps.Humanlike && pawn.Name != null && !pawn.Name.Numerical)
				{
					pawnLabel = pawn.Name.ToStringShort.CapitalizeFirst() + ", " + pawn.KindLabel;
				}

				else
				{
					pawnLabel = pawn.LabelCap;
				}

				DingoUtils.CachedStrings[pawn] = pawnLabel;
			}

			return pawnLabel;
		}

		/// <summary>
		/// Draws the DraggableOutline texture on the edges of a rect. Works best with square rects.
		/// </summary>
		public static void DraggableOutline(Rect rect, Color color)
		{
			GUI.color = color;

			GUI.DrawTexture(rect, DraggableOutlineTexture);

			GUI.color = Color.white; //Reset
		}

		/// <summary>
		/// Draws an outline around a rect using 4 individual lines. Scales with any rect.
		/// </summary>
		public static void BoxOutline(Rect rect)
		{
			GUI.color = OutlineColour;

			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), BaseContent.WhiteTex); //Ceiling
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Left wall
			GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Right wall
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - 1f, rect.width, 1f), BaseContent.WhiteTex); //Floor

			GUI.color = Color.white; //Reset
		}

		/// <summary>
		/// Draws a horizontal line to separate list items.
		/// </summary>
		public static void ListSeparator(Rect rect, float verticalPos)
		{
			GUI.color = ListSeparatorColour;
			Rect position = new Rect(rect.x, verticalPos, rect.width, 1f);

			GUI.DrawTexture(position, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		/// <summary>
		/// Draws a pawn label and provides context-sensitive JumpSelect, highlighting and tooltip handling.
		/// </summary>
		public static void PawnLabel(Rect rect, Pawn pawn, EventData data)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;

			Widgets.Label(rect, pawn.CachedPawnLabel());

			Text.Anchor = TextAnchor.UpperLeft; //Reset
			Text.WordWrap = true; //Reset

			if (rect.Contains(data.mousePosition))
			{
				if (data.type == EventType.MouseDown && data.button == 0)
				{
					CameraJumper.TryJumpAndSelect(pawn);

					//Close open DD windows (vanilla does not close on jump-select, but it seems natural)
					//Find.WindowStack.TryRemove(typeof(Window_ColonistStats), false);
					Find.WindowStack.TryRemove(typeof(Window_WorkTab), false);
				}

				Widgets.DrawHighlight(rect);

				TooltipHandler.TipRegion(rect, "ClickToJumpTo".CachedTranslation() + pawn.GetTooltip().text);
			}
		}

		/// <summary>
		/// Draws pawn Passion on the lower right side of a rect.
		/// </summary>
		public static void DrawPassion(Rect drawRect, Pawn worker, WorkTypeDef workDef)
		{
			//WidgetsWork.DrawWorkBoxBackground
			Passion passion = worker.skills.MaxPassionOfRelevantSkillsFor(workDef);

			if (passion > Passion.None)
			{
				Rect passionRect = new Rect(drawRect.xMax - PassionDrawSize - 2f, drawRect.yMax - PassionDrawSize - 2f, PassionDrawSize, PassionDrawSize);
				Texture passionTex = passion == Passion.Minor ? PassionMinorIcon : PassionMajorIcon;

				GUI.DrawTexture(passionRect, passionTex);
			}
		}

		/// <summary>
		/// Builds a context-appropriate WorkType tooltip based on various inputs.
		/// </summary>
		public static string DraggableTooltip(WorkTypeDef def, bool primary, bool statsWindow, bool incapable, bool disabled, Pawn worker)
		{
			StringBuilder tooltip = new StringBuilder(def.labelShort);

			if (primary)
			{
				tooltip.AppendLine();
				tooltip.AppendLine();
				tooltip.Append(def.description);

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_Tooltip_Primary".CachedTranslation());
				}

				else
				{
					tooltip.Append("ClickToSortByThisColumn".CachedTranslation());
				}
			}

			//RimWorld.WidgetsWork.TipForPawnWorker
			else
			{
				if (!incapable)
				{
					if (def.relevantSkills.Count > 0)
					{
						tooltip.Append("RelevantSkills".CachedTranslation(new object[]
						{
						DingoUtils.CachedStrings[def],
						worker.skills.AverageOfRelevantSkillsFor(def).ToString(),
						SkillRecord.MaxLevel
						}));
					}

					if (!statsWindow)
					{
						tooltip.Append("DD_WorkTab_Tooltip_DraggableWork".CachedTranslation());
					}

					if (disabled)
					{
						tooltip.Append("DD_WorkTab_Tooltip_CurrentlyUnassigned".CachedTranslation(new string[] { def.labelShort }).AdjustedFor(worker));
					}

					if (CapacitiesCompromisedForWorkType(worker, def))
					{
						tooltip.Append("DD_WorkTab_Tooltip_IncapacitatedWorker".CachedTranslation(new string[] { def.labelShort }).AdjustedFor(worker));
					}
				}

				else
				{
					tooltip.AppendLine();
					tooltip.AppendLine();
					tooltip.Append("DD_WorkTab_Message_IncapablePawn".CachedTranslation(new string[] { def.labelShort }).AdjustedFor(worker));
				}
			}

			return tooltip.ToString();
		}

		/// <summary>
		/// Provides work-related buttons with 'Are you sure?' prompts.
		/// </summary>
		public static void Button(Rect buttonRect, ButtonType buttonType, EventData data, PawnSurface surface)
		{
			if (data.type == EventType.Repaint)
			{
				DraggableOutline(buttonRect, MediumSkillColour);

				Rect textureRect = buttonRect.ContractedBy(2f);

				switch (buttonType)
				{
					case ButtonType.DisableWork:
						GUI.DrawTexture(textureRect, DisableWorkTexture);
						break;
					case ButtonType.ResetWork:
						GUI.DrawTexture(textureRect, ResetWorkTexture);
						break;
					case ButtonType.DisableAllWork:
						GUI.DrawTexture(textureRect, DisableWorkTexture);
						break;
					case ButtonType.ResetAllWork:
						GUI.DrawTexture(textureRect, ResetWorkTexture);
						break;
				}
			}

			if (buttonRect.Contains(data.mousePosition))
			{
				string tooltip;
				string text;
				string title;
				Action buttonAction;

				switch (buttonType)
				{
					case ButtonType.DisableWork:
						tooltip = "DD_WorkTab_Tooltip_ButtonDisableWork".CachedTranslation();
						text = "DD_WorkTab_PromptText_DisableWork".CachedTranslation().AdjustedFor(surface.pawn);
						title = "DD_WorkTab_PromptTitle_DisableWork".CachedTranslation().AdjustedFor(surface.pawn);
						buttonAction = delegate { surface.DisableAllPawnWork(); };
						break;
					case ButtonType.ResetWork:
						tooltip = "DD_WorkTab_Tooltip_ButtonResetWork".CachedTranslation();
						text = "DD_WorkTab_PromptText_ResetWork".CachedTranslation().AdjustedFor(surface.pawn);
						title = "DD_WorkTab_PromptTitle_ResetWork".CachedTranslation().AdjustedFor(surface.pawn);
						buttonAction = delegate { surface.ResetWorkToDefaultState(); };
						break;
					case ButtonType.DisableAllWork:
						tooltip = "DD_WorkTab_Tooltip_ButtonDisableWork_AllPawns".CachedTranslation();
						text = "DD_WorkTab_PromptText_DisableWork_AllPawns".CachedTranslation();
						title = "DD_WorkTab_PromptTitle_DisableWork_AllPawns".CachedTranslation();
						buttonAction = delegate
						{
							foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
							{
								Controller.GetManager.GetPawnSurface(p).DisableAllPawnWork();
							}
						};
						break;
					case ButtonType.ResetAllWork:
						tooltip = "DD_WorkTab_Tooltip_ButtonResetWork_AllPawns".CachedTranslation();
						text = "DD_WorkTab_PromptText_ResetWork_AllPawns".CachedTranslation();
						title = "DD_WorkTab_PromptTitle_ResetWork_AllPawns".CachedTranslation();
						buttonAction = delegate
						{
							foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
							{
								Controller.GetManager.GetPawnSurface(p).ResetWorkToDefaultState();
							}
						};
						break;
					default:
						tooltip = string.Empty;
						text = string.Empty;
						title = string.Empty;
						buttonAction = default(Action);
						break;
				}

				if (data.type == EventType.Repaint)
				{
					Widgets.DrawHighlight(buttonRect);

					TooltipHandler.TipRegion(buttonRect, tooltip);
				}

				else if (data.type == EventType.MouseDown && data.button == 0)
				{
					if (Controller.ShowPrompt)
					{
						DiaOption acceptButton = new DiaOption("DD_WorkTab_ButtonOption_Accept".CachedTranslation())
						{
							action = buttonAction,
							resolveTree = true
						};

						DiaOption rejectButton = new DiaOption("DD_WorkTab_ButtonOption_Cancel".CachedTranslation())
						{
							resolveTree = true
						};

						DiaOption acceptDoNotShowAgain = new DiaOption("DD_WorkTab_ButtonOption_AcceptDisablePrompt".CachedTranslation())
						{
							action = delegate
							{
								Controller.ShowPrompt.Value = false;

								buttonAction();
							},
							resolveTree = true
						};

						DiaNode prompt = new DiaNode(text + "DD_WorkTab_PromptText_DisablePrompt".CachedTranslation())
						{
							options = new List<DiaOption> { acceptButton, rejectButton, acceptDoNotShowAgain }
						};

						Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, title));
					}

					else
					{
						buttonAction();

						if (Controller.UseSounds)
						{
							TaskCompleted.PlayOneShotOnCamera(null);
						}
					}
				}
			}
		}

		/// <summary>
		/// Public bool equivalent to a private vanilla bool. Checks for incapacitation using quick HashSet lookup.
		/// </summary>
		public static bool CapacitiesCompromisedForWorkType(Pawn p, WorkTypeDef def)
		{
			//RimWorld.PawnColumnWorker_WorkPriority equivalent
			foreach (PawnCapacityDef capDef in WorkDefAttributes[def].allRequiredCapacities)
			{
				if (!p.health.capacities.CapableOf(capDef))
				{
					return true;
				}
			}

			return false;
		}
	}
}
