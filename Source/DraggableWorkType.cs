using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	/* Based on the work of Emil Johansen aka AngryAnt
	* https://github.com/AngryAnt */

	public class DraggableWorkType : IExposable
	{
		private Texture2D texture;

		private Vector2 dragOffsetVector;

		public readonly bool primary;

		public readonly PawnSurface parent;

		public WorkTypeDef def;

		public Vector2 position;

		public Rect dragRect;

		public bool draggingNow;

		private void FetchUtilityValues()
		{
			if (!this.primary)
			{
				this.texture = DD_Widgets.WorkDefAttributes[this.def].texture;

				this.parent.QuickFindByDef[this.def] = this;
			}

			else
			{
				this.texture = DD_Widgets.WorkDefAttributes[this.def].primaryTexture;
			}
		}

		private Color GetDynamicColour()
		{
			if (this.primary)
			{
				return DD_Widgets.MediumSkillColour;
			}

			if (DD_Widgets.CapacitiesCompromisedForWorkType(this.parent.pawn, this.def))
			{
				return Color.red;
			}

			if (this.def.relevantSkills.Count == 0)
			{
				return DD_Widgets.MediumSkillColour;
			}

			float skillAverage = this.parent.pawn.skills.AverageOfRelevantSkillsFor(this.def);

			if (skillAverage < 4f) //0-3
			{
				return DD_Widgets.LowSkillColour;
			}

			if (skillAverage < 8f) //4-7
			{
				return DD_Widgets.MediumSkillColour;
			}

			if (skillAverage < 12f) //8-11
			{
				return DD_Widgets.HighSkillColour;
			}

			if (skillAverage < 16f) //12-15
			{
				return DD_Widgets.VeryHighSkillColour;
			}

			return DD_Widgets.ExcellentSkillColour; //16-20
		}

		public void DrawTexture(Rect drawRect, bool drawPassion)
		{
			DD_Widgets.DraggableOutline(drawRect, this.GetDynamicColour());

			GUI.DrawTexture(drawRect.ContractedBy(2f), this.texture);

			if (drawPassion)
			{
				DD_Widgets.DrawPassion(this.parent.pawn, this.def, drawRect);
			}
		}

		public int DoWorkTabGUI(Vector2 mousePosition)
		{
			int clickInt = 0;

			this.DrawTexture(this.dragRect, !this.primary);

			if (Event.current.type == EventType.MouseUp)
			{
				this.draggingNow = false;
			}

			if (dragRect.Contains(mousePosition))
			{
				if (Event.current.type == EventType.MouseDown)
				{
					if (!Event.current.shift)
					{
						if (Event.current.button == 0)
						{
							this.draggingNow = true;
							this.dragOffsetVector = mousePosition - this.position;

							DragManager.CurrentDraggable = this;
						}
					}

					else
					{
						if (Event.current.button == 0)
						{
							clickInt = -1;
						}

						if (Event.current.button == 1)
						{
							clickInt = 1;
						}
					}

					Event.current.Use();
				}

				if (!DragManager.Dragging)
				{
					Widgets.DrawHighlight(this.dragRect);

					Pawn worker = !this.primary ? this.parent.pawn : null;

					TooltipHandler.TipRegion(this.dragRect, DD_Widgets.DraggableTooltip(this.def, false, this.primary, worker));
				}
			}

			if (draggingNow)
			{
				this.position = mousePosition - this.dragOffsetVector;

				this.dragRect = this.position.ToDraggableRect();
			}

			return clickInt;
		}

		public DraggableWorkType(PawnSurface surface)
		{
			this.parent = surface;
		}

		public DraggableWorkType(WorkTypeDef workType, PawnSurface surface, bool isPrimary, Vector2 posVector = default(Vector2)) : this(surface)
		{
			this.def = workType;
			this.primary = isPrimary;
			this.position = posVector;

			this.FetchUtilityValues();
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref this.def, "def");

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.FetchUtilityValues();
			}
		}
	}
}
