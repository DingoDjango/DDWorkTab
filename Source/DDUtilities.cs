using System.Text;
using Harmony;
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

		public static float standardSurfaceWidth = standardSpacing + DefDatabase<WorkTypeDef>.AllDefsListForReading.Count * (DraggableTextureWidth + standardSpacing);

		public static Vector2 DraggableSize = new Vector2(DraggableTextureWidth, DraggableTextureHeight); //Unused

		public static Texture2D ButtonTexture_DisableAll = ContentFinder<Texture2D>.Get("ButtonDisableAll", true);

		public static Texture2D ButtonTexture_ResetToVanilla = ContentFinder<Texture2D>.Get("ButtonResetToDefaults", true);

		public static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true);

		public static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true);

		public static readonly Texture2D HaltIcon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true); //Temporary from vanilla

		public static Color ButtonColour = new Color(1f, 0.65f, 0f); //Orange

		public static Color LowSkillColour = new Color(0.5f, 0.5f, 0.5f); //Dark grey, also used for no-skill work types

		public static Color MediumSkillColour = Color.white;

		public static Color HighSkillColour = new Color(0f, 0.6f, 1f); //Dark blue

		public static Color VeryHighSkillColour = Color.green;

		public static Color ExcellentSkillColour = new Color(1f, 0.85f, 0f); /* GOLD http://whenisnlss.com/assets/sounds/GOLD.mp3 */

		//Provides a texture from the DD WorkTab Textures folder for a given WorkTypeDef
		public static Texture2D GetDraggableTexture(this DraggableWorkType draggable)
		{
			Texture2D validTexture = ContentFinder<Texture2D>.Get("WorkTypeIcons/" + draggable.def.defName);

			if (validTexture != null)
			{
				return validTexture; //Returns a texture for a specific WorkType
			}

			else return BaseContent.BadTex; //Bad texture, currently for testing
		}

		//True if the user left clicked inside the given Rect
		public static bool MouseLeftClickedRect(Rect rect)
		{
			return Event.current.type == EventType.MouseDown
				&& Event.current.button == 0
				&& Mouse.IsOver(rect);
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
		public static void DrawOutline(Rect rect, bool isEmergency = false)
		{
			GUI.color = isEmergency ? Color.red : new Color(0.6f, 0.6f, 0.6f, 1f);

			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 1f), BaseContent.WhiteTex); //Ceiling
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Left wall
			GUI.DrawTexture(new Rect(rect.xMax - 1f, rect.yMin, 1f, rect.height), BaseContent.WhiteTex); //Right wall
			GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - 1f, rect.width, 1f), BaseContent.WhiteTex); //Floor

			GUI.color = Color.white; //Reset
		}

		//Draw light-grey horizontal line
		public static void DrawListSeparator(Rect rect, float verticalPos)
		{
			GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
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

			Widgets.DrawHighlightIfMouseover(rect);

			if (Widgets.ButtonInvisible(rect, false))
			{
				CameraJumper.TryJumpAndSelect(pawn);

				//Close open DD windows (vanilla does not close on jump-select, but it seems natural to do it)
				Find.WindowStack.TryRemove(typeof(Window_ColonistStats), false);
				Find.WindowStack.TryRemove(typeof(Window_WorkTab), false);
			}

			string tooltip = "ClickToJumpTo".Translate() + "\n\n" + pawn.GetTooltip().text;
			TooltipHandler.TipRegion(rect, tooltip);
		}

		public static string GetDraggableTooltip(this DraggableWorkType draggable, bool statsWindow)
		{
			WorkTypeDef def = draggable.def;

			StringBuilder tooltip = new StringBuilder(def.gerundLabel);

			if (draggable.isEmergency)
			{
				tooltip.Append("DD_WorkTab_WorkTypeHasEmergency".Translate());
			}

			if (draggable.isPrimaryType)
			{
				tooltip.AppendLine("\n\n" + def.description);

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_PrimeDraggable_DragTip".Translate(new string[] { def.gerundLabel }));
				}

				else
				{
					tooltip.Append("\n" + "ClickToSortByThisColumn".Translate());
				}
			}

			//RimWorld.WidgetsWork.TipForPawnWorker
			else
			{
				Pawn worker = draggable.parent.pawn;

				if (def.relevantSkills.Count > 0)
				{
					string relevantSkills = string.Empty;

					foreach (SkillDef skill in def.relevantSkills)
					{
						relevantSkills = relevantSkills + skill.skillLabel + ", ";
					}

					relevantSkills = relevantSkills.Substring(0, relevantSkills.Length - 2); //Deletes the last ", "

					tooltip.Append("\n\n" + "RelevantSkills".Translate(new object[]
					{
						relevantSkills,
						worker.skills.AverageOfRelevantSkillsFor(def).ToString(),
						SkillRecord.MaxLevel
					}));
				}

				PawnColumnWorker_WorkPriority columnWorker = new PawnColumnWorker_WorkPriority();
				bool incapacitated = (bool)AccessTools.Method(typeof(PawnColumnWorker_WorkPriority), "IsIncapableOfWholeWorkType").Invoke(columnWorker, new object[] { worker, def });

				if (incapacitated)
				{
					tooltip.Append("\n\n" + "IncapableOfWorkTypeBecauseOfCapacities".Translate());
				}

				if (!statsWindow)
				{
					tooltip.Append("DD_WorkTab_PawnDraggable_DragTip".Translate());
				}
			}

			return tooltip.ToString();
		}

		public static Color GetDraggableColour(this DraggableWorkType d)
		{
			if (d.isPrimaryType)
			{
				return ButtonColour;
			}

			else
			{
				if (d.def.relevantSkills.Count == 0)
				{
					return LowSkillColour;
				}

				float skillAverage = d.parent.pawn.skills.AverageOfRelevantSkillsFor(d.def) / (float)SkillRecord.MaxLevel;

				if (skillAverage < 0.2f) //0-3
				{
					return LowSkillColour;
				}

				else if (skillAverage < 0.4f) //4-7
				{
					return MediumSkillColour;
				}

				else if (skillAverage < 0.6f) //8-11
				{
					return HighSkillColour;
				}

				else if (skillAverage < 0.8f) //12-15
				{
					return VeryHighSkillColour;
				}

				else //16-20
				{
					return ExcellentSkillColour;
				}
			}
		}
	}
}
