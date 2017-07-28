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

		public static Vector2 DraggableSize = new Vector2(DraggableTextureWidth, DraggableTextureHeight);

		public static Texture2D ButtonTexture_DisableAll = ContentFinder<Texture2D>.Get("ButtonDisableAll", true);

		public static Texture2D ButtonTexture_ResetToVanilla = ContentFinder<Texture2D>.Get("ButtonResetToDefaults", true);

		public static readonly Texture2D SortingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Sorting", true);

		public static readonly Texture2D SortingDescendingIcon = ContentFinder<Texture2D>.Get("UI/Icons/SortingDescending", true);

		public static readonly Texture2D HaltIcon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true); //Temporary from vanilla...

		public static Color HighlightColour = new Color(0f, 0.6f, 1f);

		public static Color DraggableColour = new Color(0.9f, 0.9f, 0.9f);

		public static Color ButtonColour = new Color(1f, 0.65f, 0f, 1f); //orange

		public static Color white = Color.white;

		public static Color gold = new Color(1f, 0.85f, 0f);

		//Get the current scrollPosition of the DD Work Tab window
		public static Vector2 TabScrollPosition
		{
			get
			{
				return Find.WindowStack.WindowOfType<Window_WorkTab>().ScrollPosition;
			}
		}

		//Provides a texture from the DD WorkTab Textures folder for a given WorkTypeDef
		public static Texture2D TextureFromModFolder(WorkTypeDef def)
		{
			Texture2D textureFromModFolder = ContentFinder<Texture2D>.Get("WorkTypeIcons/" + def.defName);

			if (textureFromModFolder != null)
			{
				return textureFromModFolder; //Returns a texture for a specific WorkType
			}

			else return ContentFinder<Texture2D>.Get("WorkTypeIcons/GenericTypeTex"); //Returns a generic WorkType texture
		}

		//True if the user left clicked inside the given Rect
		public static bool MouseLeftClickedRect(Rect rect)
		{
			return Event.current.type == EventType.MouseDown
				&& Event.current.button == 0
				&& Mouse.IsOver(rect);
		}

		//Provides a Rect whose center point is Vector2 "position"
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

		public static void DrawOutline(Rect rect)
		{
			GUI.color = new Color(0.6f, 0.6f, 0.6f, 1f);

			Widgets.DrawBox(rect, 1);

			GUI.color = Color.white; //Reset
		}

		public static void DrawListSeparator(Rect rect, float verticalPos)
		{
			GUI.color = new Color(0.3f, 0.3f, 0.3f, 1f);
			Rect position = new Rect(rect.x, verticalPos, rect.width, 1f);

			GUI.DrawTexture(position, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		public static void DoPawnLabel(this Window window, Rect rect, Pawn pawn)
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

			TipSignal tooltip = pawn.GetTooltip();
			tooltip.text = "ClickToJumpTo".Translate() + "\n\n" + tooltip.text;
			TooltipHandler.TipRegion(rect, tooltip);
		}

		public static string GetDraggableTooltip(this WorkTypeDef def, bool isPrimary, Pawn worker)
		{
			StringBuilder tooltip = new StringBuilder();
			tooltip.AppendLine(def.gerundLabel);
			tooltip.AppendLine();

			//RimWorld.PawnColumnWorker_WorkPriority.GetHeaderTip
			if (isPrimary)
			{
				tooltip.AppendLine(def.description);
				tooltip.AppendLine();

				for (int i = 0; i < def.workGiversByPriority.Count; i++)
				{
					WorkGiverDef currentWorkGiver = def.workGiversByPriority[i];

					tooltip.Append(currentWorkGiver.LabelCap);

					if (currentWorkGiver.emergency)
					{
						tooltip.Append(" (" + "EmergencyWorkMarker".Translate() + ")");
					}

					if (i < def.workGiversByPriority.Count - 1)
					{
						tooltip.AppendLine();
					}
				}
			}

			//RimWorld.WidgetsWork.TipForPawnWorker
			else
			{
				if (def.relevantSkills.Count > 0)
				{
					string relevantSkills = string.Empty;

					foreach (SkillDef skill in def.relevantSkills)
					{
						relevantSkills = relevantSkills + skill.skillLabel + ", ";
					}
					relevantSkills = relevantSkills.Substring(0, relevantSkills.Length - 2);

					tooltip.AppendLine("RelevantSkills".Translate(new object[]
					{
						relevantSkills,
						worker.skills.AverageOfRelevantSkillsFor(def).ToString(),
						SkillRecord.MaxLevel
					}));

					tooltip.AppendLine();
				}

				tooltip.Append(def.description);

				PawnColumnWorker_WorkPriority columnWorker = new PawnColumnWorker_WorkPriority();
				bool incapacitated = (bool)AccessTools.Method(typeof(PawnColumnWorker_WorkPriority), "IsIncapableOfWholeWorkType").Invoke(columnWorker, new object[] { worker, def });

				if (incapacitated)
				{
					tooltip.AppendLine();
					tooltip.AppendLine();
					tooltip.Append("IncapableOfWorkTypeBecauseOfCapacities".Translate());
				}
			}

			return tooltip.ToString();
		}

		public static Color AdjustedForPawnSkills(this Color col, Pawn p, WorkTypeDef w)
		{
			if (w.relevantSkills.Count == 0)
			{
				return new Color(col.r, col.g, col.b, 0.7f);
			}

			float skillAverage = p.skills.AverageOfRelevantSkillsFor(w) / (float)SkillRecord.MaxLevel;

			if (skillAverage <= 0.2f)
			{
				return new Color(col.r, col.g, col.b, 0.7f);
			}

			else if (skillAverage <= 0.5f)
			{
				return new Color(white.r, white.g, white.b, 0.7f);
			}

			else if (skillAverage <= 0.8f)
			{
				return white;
			}

			else
			{
				return gold;
			}
		}
	}
}
