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
		public const float DraggableDiameter = 48f;

		public const float SmallDraggableDiameter = 32f;

		public const float PassionDrawSize = 14f;

		public const float ShortSpacing = 8f;

		public const float SpaceForPawnLabel = 140f;

		public const float SpaceForScrollBar = 22f;

		public const float SpaceForWorkButton = 2f * ShortSpacing + DraggableDiameter;

		public const float StandardRowHeight = 2f * ShortSpacing + DraggableDiameter;

		public const float SmallRowHeight = 2f * ShortSpacing + SmallDraggableDiameter;

		public static readonly float PawnSurfaceWidth = 2f * ShortSpacing + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DraggableDiameter + ShortSpacing);

		public static readonly float SmallSurfaceWidth = ShortSpacing + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (SmallDraggableDiameter + ShortSpacing);

		public static readonly float TinyTextLineHeight;

		public static readonly Texture2D DraggableOutlineTexture;

		public static readonly Texture2D WorkButtonTexture;

		public static readonly Texture2D SortingDescendingIcon;

		public static readonly Texture2D SortingAscendingIcon;

		public static readonly Texture2D PassionMinorIcon;

		public static readonly Texture2D PassionMajorIcon;

		public static readonly Texture2D IncapableWorkerX;

		public static readonly Color OutlineColour = new Color(0.6f, 0.6f, 0.6f); //Light grey

		public static readonly Color ListSeparatorColour = new Color(0.3f, 0.3f, 0.3f); //Faded grey

		public static readonly Color IndicatorsColour = new Color(1f, 1f, 1f, 0.5f); //Transparent grey

		public static readonly Color LowSkillColour = new Color(0.8f, 0.8f, 0.8f, 0.5f); //Grey

		public static readonly Color MediumSkillColour = Color.white;

		public static readonly Color HighSkillColour = new Color(0f, 0.6f, 1f); //Dark blue

		public static readonly Color VeryHighSkillColour = Color.green;

		public static readonly Color ExcellentSkillColour = new Color(1f, 0.85f, 0f); /* GOLD http://whenisnlss.com/assets/sounds/GOLD.mp3 */

		public static readonly Color Orange = new Color(1f, 0.65f, 0f);

		public static readonly SoundDef TaskCompleted = MessageTypeDefOf.PositiveEvent.sound;

		public static readonly SoundDef TaskFailed = SoundDefOf.ClickReject;

		public static readonly SoundDef WorkEnabled = SoundDefOf.LessonActivated;

		public static readonly SoundDef WorkDisabled = SoundDefOf.LessonDeactivated;

		public static readonly SoundDef SortedSkills = SoundDefOf.TickHigh;

		public static readonly SoundDef UnsortedSkills = SoundDefOf.TickLow;

		public static readonly SoundDef CompareSkillsMapChanged = SoundDefOf.DialogBoxAppear;

		public static readonly Dictionary<WorkTypeDef, WorkTypeInfo> WorkDefAttributes = new Dictionary<WorkTypeDef, WorkTypeInfo>();

		//CachedStrings[string] => cached translation
		//CachedStrings[WorkTypeDef] => relevant skills for def
		//CachedStrings[Pawn] => pawn label
		public static Dictionary<object, string> CachedStrings = new Dictionary<object, string>();

		public static float MaxWindowWidth => UI.screenWidth * 0.8f;

		public static float MaxWindowHeight => UI.screenHeight * 0.8f;

		static Utilities()
		{
			Text.Font = GameFont.Tiny;
			TinyTextLineHeight = Text.LineHeight;
			Text.Font = GameFont.Small; //Reset

			//Modify vanilla translations for better tooltip building
			CachedStrings["ClickToSortByThisColumn"] = "\n\n" + "ClickToSortByThisColumn".Translate();
			CachedStrings["RelevantSkills"] = "\n\n" + "RelevantSkills".Translate();
			CachedStrings["ClickToJumpTo"] = "ClickToJumpTo".Translate() + "\n\n";

			DraggableOutlineTexture = DingoUtils.GetHQTexture("DraggableOutline");
			WorkButtonTexture = DingoUtils.GetHQTexture("Cog");
			SortingDescendingIcon = DingoUtils.GetHQTexture("SortingDescending");
			SortingAscendingIcon = DingoUtils.GetHQTexture("Sorting");
			PassionMinorIcon = DingoUtils.GetHQTexture("PassionMinor");
			PassionMajorIcon = DingoUtils.GetHQTexture("PassionMajor");
			IncapableWorkerX = DingoUtils.GetHQTexture("IncapableWorkerX");

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
					string relevantSkills = "";

					for (int k = 0; k < def.relevantSkills.Count; k++)
					{
						relevantSkills += def.relevantSkills[k].skillLabel + ", ";
					}

					CachedStrings[def] = relevantSkills.Substring(0, relevantSkills.Length - 2);
				}
			}

			Controller.GetPrimaries = new PrimarySurface();
		}

		/// <summary>
		/// Provides quick storage and access to translations. Circumvents calling the .Translate() chain more than once.
		/// </summary>
		public static string CachedTranslation(this string inputText, object[] args = null)
		{
			if (!CachedStrings.TryGetValue(inputText, out string finalString))
			{
				finalString = inputText.Translate();

				CachedStrings[inputText] = finalString;
			}

			if (args != null)
			{
				return string.Format(finalString, args);
			}

			return finalString;
		}

		/// <summary>
		/// Provides a Draggable-sized square whose center point is the provided position.
		/// </summary>
		public static Rect ToWorkRect(this Vector2 position, float diameter = DraggableDiameter)
		{
			float draggableRadius = diameter / 2f;

			return new Rect(position.x - draggableRadius, position.y - draggableRadius, diameter, diameter);
		}

		/// <summary>
		/// Quickly fetches a pawn's cached name string.
		/// </summary>
		public static string CachedPawnLabel(this Pawn pawn)
		{
			if (!CachedStrings.TryGetValue(pawn, out string pawnLabel))
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

				CachedStrings[pawn] = pawnLabel;
			}

			return pawnLabel;
		}

		/// <summary>
		/// Centralised method to send messages and sounds to the user.
		/// </summary>
		public static void UserFeedbackChain(WorkSound workSound, string message = "")
		{
			if (Controller.UseSounds)
			{
				SoundDef audio = null;

				switch (workSound)
				{
					case (WorkSound.TaskCompleted):
						audio = TaskCompleted;
						break;
					case (WorkSound.TaskFailed):
						audio = TaskFailed;
						break;
					case (WorkSound.WorkEnabled):
						audio = WorkEnabled;
						break;
					case (WorkSound.WorkDisabled):
						audio = WorkDisabled;
						break;
					case (WorkSound.SortedSkills):
						audio = SortedSkills;
						break;
					case (WorkSound.UnsortedSkills):
						audio = UnsortedSkills;
						break;
					case (WorkSound.CompareSkillsMapChanged):
						audio = CompareSkillsMapChanged;
						break;
				}

				audio.PlayOneShotOnCamera(null);
			}

			if (message != "" && Controller.VerboseMessages)
			{
				Messages.Message(message, MessageTypeDefOf.SilentInput);
			}
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
		/// Draws a pawn label using a cached name string.
		/// </summary>
		public static void PawnLabel(Rect rect, Pawn pawn)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;

			Widgets.Label(rect, pawn.CachedPawnLabel());

			Text.Anchor = TextAnchor.UpperLeft; //Reset
			Text.WordWrap = true; //Reset

			if (rect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.Repaint)
				{
					Widgets.DrawHighlight(rect);

					TooltipHandler.TipRegion(rect, "ClickToJumpTo".CachedTranslation() + pawn.GetTooltip().text);
				}

				else if (Event.current.type == EventType.MouseDown)
				{
					CameraJumper.TryJumpAndSelect(pawn);

					//Close active Work windows
					Find.WindowStack.TryRemove(typeof(Window_WorkTab), false);
					Find.WindowStack.TryRemove(typeof(Window_ColonistSkills), false);
				}
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
		/// Builds an appropriate tooltip for a PrimaryWork object.
		/// </summary>
		public static string PrimaryTooltip(PrimaryWork primary, bool compareSkillsWindow)
		{
			StringBuilder tooltip = new StringBuilder(primary.def.labelShort);

			tooltip.AppendLine();
			tooltip.AppendLine();
			tooltip.Append(primary.def.description);

			if (!compareSkillsWindow)
			{
				tooltip.Append("DD_WorkTab_Tooltip_Primary".CachedTranslation());
			}

			else
			{
				tooltip.Append("ClickToSortByThisColumn".CachedTranslation());
			}

			return tooltip.ToString();
		}

		/// <summary>
		/// Builds an appropriate tooltip for a DraggableWork object.
		/// </summary>
		public static string DraggableTooltip(DraggableWork draggable, bool compareSkillsWindow)
		{
			WorkTypeDef def = draggable.Def;
			Pawn worker = draggable.parent.pawn;

			StringBuilder tooltip = new StringBuilder(def.labelShort);

			//RimWorld.WidgetsWork.TipForPawnWorker
			if (!draggable.CompletelyDisabled)
			{
				if (def.relevantSkills.Count > 0)
				{
					tooltip.Append("RelevantSkills".CachedTranslation(new object[]
					{
						CachedStrings[def],
						worker.skills.AverageOfRelevantSkillsFor(def).ToString(),
						SkillRecord.MaxLevel
					}));
				}

				if (!compareSkillsWindow)
				{
					tooltip.Append("DD_WorkTab_Tooltip_DraggableWork".CachedTranslation());
				}

				if (draggable.Disabled)
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

			return tooltip.ToString();
		}

		/// <summary>
		/// A master button for all WorkFunction options.
		/// </summary>
		public static void WorkButton(Rect buttonRect, PawnSurface surface)
		{
			if (Event.current.type == EventType.Repaint)
			{
				DraggableOutline(buttonRect, MediumSkillColour);

				GUI.DrawTexture(buttonRect.ContractedBy(2f), WorkButtonTexture);
			}

			if (buttonRect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.Repaint)
				{
					Widgets.DrawHighlight(buttonRect);

					TooltipHandler.TipRegion(buttonRect, "DD_WorkTab_Tooltip_WorkButton".CachedTranslation());
				}

				else if (Event.current.type == EventType.MouseDown)
				{
					Find.WindowStack.Add(WorkMenu(surface));
				}
			}
		}

		/// <summary>
		/// Creates a FloatMenu based on Primary/PawnSurface options.
		/// </summary>
		public static FloatMenu WorkMenu(PawnSurface surface)
		{
			List<FloatMenuOption> floatOptions = new List<FloatMenuOption>();

			if (surface == null)
			{
				floatOptions.Add(WorkOption(WorkFunction.AllPawns_EnableWork, null));
				floatOptions.Add(WorkOption(WorkFunction.AllPawns_DisableWork, null));
				floatOptions.Add(WorkOption(WorkFunction.AllPawns_ResetWork, null));
			}

			else
			{
				floatOptions.Add(WorkOption(WorkFunction.EnableWork, surface));
				floatOptions.Add(WorkOption(WorkFunction.DisableWork, surface));
				floatOptions.Add(WorkOption(WorkFunction.ResetWork, surface));
				floatOptions.Add(WorkOption(WorkFunction.CopySettings, surface));
				floatOptions.Add(WorkOption(WorkFunction.PasteSettings, surface));
			}

			return new FloatMenu(floatOptions);
		}

		/// <summary>
		/// Builds a Float Menu entry to perform various work functions. 
		/// </summary>
		public static FloatMenuOption WorkOption(WorkFunction function, PawnSurface surface)
		{
			string title = "";
			Action buttonAction = default(Action);

			switch (function)
			{
				case WorkFunction.AllPawns_EnableWork:
					title = "DD_WorkTab_Title_AllPawns_EnableWork".CachedTranslation();
					buttonAction = delegate
					{
						foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
						{
							Controller.GetManager.GetPawnSurface(p).EnableAllPawnWork();
						}
					};
					break;
				case WorkFunction.AllPawns_DisableWork:
					title = "DD_WorkTab_Title_AllPawns_DisableWork".CachedTranslation();
					buttonAction = delegate
					{
						foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
						{
							Controller.GetManager.GetPawnSurface(p).DisableAllPawnWork();
						}
					};
					break;
				case WorkFunction.AllPawns_ResetWork:
					title = "DD_WorkTab_Title_AllPawns_ResetWork".CachedTranslation();
					buttonAction = delegate
					{
						foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
						{
							Controller.GetManager.GetPawnSurface(p).ResetAllPawnWork();
						}
					};
					break;
				case WorkFunction.EnableWork:
					title = "DD_WorkTab_Title_EnableWork".CachedTranslation();
					buttonAction = delegate
					{
						surface.EnableAllPawnWork();
					};
					break;
				case WorkFunction.DisableWork:
					title = "DD_WorkTab_Title_DisableWork".CachedTranslation();
					buttonAction = delegate
					{
						surface.DisableAllPawnWork();
					};
					break;
				case WorkFunction.ResetWork:
					title = "DD_WorkTab_Title_ResetWork".CachedTranslation();
					buttonAction = delegate
					{
						surface.ResetAllPawnWork();
					};
					break;
				case WorkFunction.CopySettings:
					title = "DD_WorkTab_Title_CopyWorkPriorities".CachedTranslation();
					buttonAction = delegate
					{
						surface.CopyPriorities();
					};
					break;
				case WorkFunction.PasteSettings:
					title = "DD_WorkTab_Title_PasteWorkPriorities".CachedTranslation();
					buttonAction = delegate
					{
						surface.PastePriorities(Controller.CopyPrioritiesReference);
					};
					break;
			}

			if (Controller.ShowPrompts && function != WorkFunction.CopySettings && function != WorkFunction.PasteSettings)
			{
				return new FloatMenuOption(title, delegate
				{
					WorkPrompt(function, buttonAction, title, surface);
				});
			}

			return new FloatMenuOption(title, delegate
			{
				buttonAction();

				UserFeedbackChain(WorkSound.TaskCompleted);
			});
		}

		/// <summary>
		/// Open a pop-up prompt window for button functions.
		/// </summary>
		public static void WorkPrompt(WorkFunction function, Action buttonAction, string title, PawnSurface surface)
		{
			string text = "";

			switch (function)
			{
				case WorkFunction.AllPawns_EnableWork:
					text = "DD_WorkTab_PromptText_AllPawns_EnableWork".CachedTranslation();
					break;
				case WorkFunction.AllPawns_DisableWork:
					text = "DD_WorkTab_PromptText_AllPawns_DisableWork".CachedTranslation();
					break;
				case WorkFunction.AllPawns_ResetWork:
					text = "DD_WorkTab_PromptText_AllPawns_ResetWork".CachedTranslation();
					break;
				case WorkFunction.EnableWork:
					text = "DD_WorkTab_PromptText_EnableWork".CachedTranslation().AdjustedFor(surface.pawn);
					break;
				case WorkFunction.DisableWork:
					text = "DD_WorkTab_PromptText_DisableWork".CachedTranslation().AdjustedFor(surface.pawn);
					break;
				case WorkFunction.ResetWork:
					text = "DD_WorkTab_PromptText_ResetWork".CachedTranslation().AdjustedFor(surface.pawn);
					break;
			}

			DiaOption accept = new DiaOption("DD_WorkTab_PromptOption_Accept".CachedTranslation())
			{
				action = buttonAction,
				resolveTree = true
			};

			DiaOption cancel = new DiaOption("DD_WorkTab_PromptOption_Cancel".CachedTranslation())
			{
				resolveTree = true
			};

			DiaOption acceptDisablePrompt = new DiaOption("DD_WorkTab_PromptOption_AcceptDisablePrompt".CachedTranslation())
			{
				action = delegate
				{
					Controller.ShowPrompts.Value = false;

					buttonAction();
				},
				resolveTree = true
			};

			DiaNode prompt = new DiaNode(text + "DD_WorkTab_PromptText_DisablePrompt".CachedTranslation())
			{
				options = new List<DiaOption> { accept, cancel, acceptDisablePrompt }
			};

			Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, title));
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
