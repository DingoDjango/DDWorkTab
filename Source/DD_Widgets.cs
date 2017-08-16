using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	[StaticConstructorOnStartup]
	public static class DD_Widgets
	{
		public const float DraggableTextureDiameter = 30f;

		public const float SpaceForPawnLabel = 140f;

		public const float SpaceForWorkButtons = 80f;

		public const float StandardSpacing = 8f;

		public const float StandardRowHeight = 2f * StandardSpacing + DraggableTextureDiameter;

		public static readonly float PawnSurfaceWidth = StandardSpacing + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DraggableTextureDiameter + StandardSpacing);

		public static readonly float TinyTextLineHeight;

		public static readonly Texture2D DraggableOutlineTexture = ContentFinder<Texture2D>.Get("Draggables/DraggableOutline", true);

		public static readonly Texture2D DisableWorkTexture = ContentFinder<Texture2D>.Get("Buttons/DisableWork", true);

		public static readonly Texture2D ResetWorkTexture = ContentFinder<Texture2D>.Get("Buttons/ResetWork", true);

		public static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true); //Vanilla

		public static readonly Texture2D SortingAscendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true); //Vanilla

		public static readonly Texture2D PassionMinorIcon = ContentFinder<Texture2D>.Get("Icons/PassionMinor", true); //Edited vanilla texture

		public static readonly Texture2D PassionMajorIcon = ContentFinder<Texture2D>.Get("Icons/PassionMajor", true); //Edited vanilla texture

		public static readonly Color OutlineColour = new Color(0.6f, 0.6f, 0.6f); //Light grey

		public static readonly Color ListSeparatorColour = new Color(0.3f, 0.3f, 0.3f); //Faded grey

		public static readonly Color IndicatorsColour = new Color(1f, 1f, 1f, 0.5f); //Transparent grey

		public static readonly Color LowSkillColour = new Color(0.8f, 0.8f, 0.8f, 0.5f); //Grey

		public static readonly Color MediumSkillColour = Color.white;

		public static readonly Color HighSkillColour = new Color(0f, 0.6f, 1f); //Dark blue

		public static readonly Color VeryHighSkillColour = Color.green;

		public static readonly Color ExcellentSkillColour = new Color(1f, 0.85f, 0f); /* GOLD http://whenisnlss.com/assets/sounds/GOLD.mp3 */

		public static readonly Dictionary<WorkTypeDef, DD_WorkTypeInfo> WorkDefAttributes = new Dictionary<WorkTypeDef, DD_WorkTypeInfo>();

		//CachedStrings[string] => cached translation
		//CachedStrings[WorkTypeDef] => relevant skills for def
		//CachedStrings[Pawn] => pawn label
		public static Dictionary<object, string> CachedStrings = new Dictionary<object, string>();

		public static float MaxWindowWidth => (float)UI.screenWidth - 10f;

		public static float MaxWindowHeight => (float)UI.screenHeight * 0.75f;

		static DD_Widgets()
		{
			foreach (WorkTypeDef def in DefDatabase<WorkTypeDef>.AllDefs)
			{
				HashSet<PawnCapacityDef> allRequiredCapacities = new HashSet<PawnCapacityDef>();

				foreach (WorkGiverDef workGiver in def.workGiversByPriority)
				{
					foreach (PawnCapacityDef capacity in workGiver.requiredCapacities)
					{
						allRequiredCapacities.Add(capacity);
					}
				}

				WorkDefAttributes[def] = new DD_WorkTypeInfo(ContentFinder<Texture2D>.Get("Draggables/" + def.defName, true), //Greyscale texture
					ContentFinder<Texture2D>.Get("Draggables/Primaries/" + def.defName, true), //Coloured texture (for primaries)
					allRequiredCapacities);

				if (def.relevantSkills.Count > 0)
				{
					string relevantSkills = string.Empty;

					foreach (SkillDef skill in def.relevantSkills)
					{
						relevantSkills += skill.skillLabel + ", ";
					}

					CachedStrings[def] = relevantSkills.Substring(0, relevantSkills.Length - 2);
				}
			}

			//Strings from vanilla
			CachedStrings["ClickToSortByThisColumn"] = "\n\n" + "ClickToSortByThisColumn".Translate();
			CachedStrings["RelevantSkills"] = "\n\n" + "RelevantSkills".Translate();
			CachedStrings["ClickToJumpTo"] = "ClickToJumpTo".Translate() + "\n\n";

			Text.Font = GameFont.Tiny;
			TinyTextLineHeight = Text.LineHeight;
			Text.Font = GameFont.Small; //Reset
		}

		//Provides a square whose center point is the provided position
		public static Rect ToDraggableRect(this Vector2 position)
		{
			float draggableRadius = DraggableTextureDiameter / 2f;

			return new Rect(position.x - draggableRadius, position.y - draggableRadius, DraggableTextureDiameter, DraggableTextureDiameter);
		}

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

		public static void DraggableOutline(Rect rect, Color color)
		{
			GUI.color = color;

			GUI.DrawTexture(rect, DraggableOutlineTexture);

			GUI.color = Color.white; //Reset
		}

		public static void BoxOutline(Rect rect)
		{
			GUI.color = OutlineColour;

			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), BaseContent.WhiteTex); //Ceiling
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Left wall
			GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Right wall
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - 1f, rect.width, 1f), BaseContent.WhiteTex); //Floor

			GUI.color = Color.white; //Reset
		}

		public static void ListSeparator(Rect rect, float verticalPos)
		{
			GUI.color = ListSeparatorColour;
			Rect position = new Rect(rect.x, verticalPos, rect.width, 1f);

			GUI.DrawTexture(position, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		public static void PawnLabel(Rect rect, Pawn pawn, Vector2 mousePosition)
		{
			Text.Font = GameFont.Small;
			Text.Anchor = TextAnchor.MiddleLeft;
			Text.WordWrap = false;

			Widgets.Label(rect, pawn.CachedPawnLabel());

			Text.Anchor = TextAnchor.UpperLeft; //Reset
			Text.WordWrap = true; //Reset

			if (rect.Contains(mousePosition))
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					CameraJumper.TryJumpAndSelect(pawn);

					//Close open DD windows (vanilla does not close on jump-select, but it seems natural)
					Find.WindowStack.TryRemove(typeof(Window_ColonistStats), false);
					Find.WindowStack.TryRemove(typeof(Window_WorkTab), false);
				}

				Widgets.DrawHighlight(rect);

				TooltipHandler.TipRegion(rect, "ClickToJumpTo".CachedTranslation() + pawn.GetTooltip().text);
			}
		}

		//WidgetsWork.DrawWorkBoxBackground
		public static void DrawPassion(Pawn worker, WorkTypeDef workDef, Rect drawRect)
		{
			Passion passion = worker.skills.MaxPassionOfRelevantSkillsFor(workDef);

			if (passion > Passion.None)
			{
				Rect passionRect = new Rect(drawRect.center.x, drawRect.center.y, 14f, 14f);
				Texture passionTex = passion == Passion.Minor ? PassionMinorIcon : PassionMajorIcon;

				GUI.DrawTexture(passionRect, passionTex, ScaleMode.ScaleToFit);
			}
		}

		public static string DraggableTooltip(WorkTypeDef def, bool statsWindow, bool primary, Pawn worker)
		{
			StringBuilder tooltip = new StringBuilder(def.labelShort);

			if (primary)
			{
				tooltip.Append("\n\n");
				tooltip.Append(def.description);

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_PrimeDraggable_DragTip".CachedTranslation());
				}

				else
				{
					tooltip.Append("ClickToSortByThisColumn".CachedTranslation());
				}
			}

			//RimWorld.WidgetsWork.TipForPawnWorker
			else
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

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_PawnDraggable_DragTip".CachedTranslation());
				}

				if (CapacitiesCompromisedForWorkType(worker, def))
				{
					tooltip.Append("DD_WorkTab_ColonistHealthCompromised".CachedTranslation().AdjustedFor(worker));
				}
			}

			return tooltip.ToString();
		}

		public static void Button(ButtonType buttonType, PawnSurface surface, Rect buttonRect, Vector2 mousePosition)
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

			if (buttonRect.Contains(mousePosition))
			{
				string tooltip;
				string text;
				string title;
				Action buttonAction;

				switch (buttonType)
				{
					case ButtonType.DisableWork:
						tooltip = "DD_WorkTab_DisableWork_Tooltip".CachedTranslation();
						text = "DD_WorkTab_DisableWork_Text".CachedTranslation().AdjustedFor(surface.pawn);
						title = "DD_WorkTab_DisableWork_Title".CachedTranslation().AdjustedFor(surface.pawn);
						buttonAction = surface.DisablePawnWork;
						break;
					case ButtonType.ResetWork:
						tooltip = "DD_WorkTab_ResetWork_Tooltip".CachedTranslation();
						text = "DD_WorkTab_ResetWork_Text".CachedTranslation().AdjustedFor(surface.pawn);
						title = "DD_WorkTab_ResetWork_Title".CachedTranslation().AdjustedFor(surface.pawn);
						buttonAction = surface.ResetPawnWorkByDefaults;
						break;
					case ButtonType.DisableAllWork:
						tooltip = "DD_WorkTab_DisableWorkVisibleMap_Tooltip".CachedTranslation();
						text = "DD_WorkTab_DisableWorkVisibleMap_Text".CachedTranslation();
						title = "DD_WorkTab_DisableWorkVisibleMap_Title".CachedTranslation();
						buttonAction = delegate
						{
							foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
							{
								DragManager.GetPawnSurface(p).DisablePawnWork();
							}
						};
						break;
					case ButtonType.ResetAllWork:
						tooltip = "DD_WorkTab_ResetWorkVisibleMap_Tooltip".CachedTranslation();
						text = "DD_WorkTab_ResetWorkVisibleMap_Text".CachedTranslation();
						title = "DD_WorkTab_ResetWorkVisibleMap_Title".CachedTranslation();
						buttonAction = delegate
						{
							foreach (Pawn p in Find.VisibleMap.mapPawns.FreeColonists)
							{
								DragManager.GetPawnSurface(p).ResetPawnWorkByDefaults();
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

				if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
				{
					if (DD_Settings.ShowPrompt)
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
								DD_Settings.ShowPrompt = false;

								buttonAction();
							},
							resolveTree = true
						};

						DiaNode prompt = new DiaNode(text + "DD_WorkTab_ButtonText_DisablePrompt".CachedTranslation())
						{
							options = new List<DiaOption> { acceptButton, rejectButton, acceptDoNotShowAgain }
						};

						Find.WindowStack.Add(new Dialog_NodeTree(prompt, false, false, title));
					}

					else
					{
						buttonAction();
					}
				}

				Widgets.DrawHighlight(buttonRect);

				TooltipHandler.TipRegion(buttonRect, tooltip);
			}
		}

		//RimWorld.PawnColumnWorker_WorkPriority equivalent
		public static bool CapacitiesCompromisedForWorkType(Pawn p, WorkTypeDef def)
		{
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
