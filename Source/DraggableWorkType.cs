using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class DraggableWorkType : DraggableObject, IExposable
	{
		public WorkTypeDef def;

		public PawnSurface parent;

		public readonly bool primary;

		public void OnWorkTabGUI()
		{
			Rect dragRect = this.position.ToDraggableRect();

			//Draw texture on draggable position
			this.DrawTexture(dragRect);

			//Do drag calculations
			this.CheckForDrag(dragRect);

			//Update DragManager if this object is being dragged
			if (this.draggingNow)
			{
				if (Dragger.CurrentDraggable != this)
				{
					Dragger.CurrentDraggable = this;
				}
			}

			if (Mouse.IsOver(dragRect) && !Dragger.Dragging)
			{
				Pawn worker = this.primary ? null : this.parent.pawn;

				Widgets.DrawHighlight(dragRect);

				TooltipHandler.TipRegion(dragRect, DDUtilities.GetDraggableTooltip(this.def, false, this.primary, worker));
			}
		}

		public void DrawTexture(Rect drawRect)
		{
			DDUtilities.DrawOutline(drawRect, DDUtilities.EmergencyWorkTypes[this.def], false);

			//Adjust colour based on isPrimary, pawn skills, work type
			GUI.color = this.GetDraggableColour();

			GUI.DrawTexture(drawRect.ContractedBy(2f), DDUtilities.WorkTypeTextures[this.def]);

			GUI.color = Color.white; //Reset
		}

		private Color GetDraggableColour()
		{
			if (this.primary)
			{
				return DDUtilities.ButtonColour;
			}

			else
			{
				if (this.def.relevantSkills.Count == 0)
				{
					return DDUtilities.LowSkillColour;
				}

				float skillAverage = this.parent.pawn.skills.AverageOfRelevantSkillsFor(this.def) / (float)SkillRecord.MaxLevel;

				if (skillAverage < 0.2f) //0-3
				{
					return DDUtilities.LowSkillColour;
				}

				else if (skillAverage < 0.4f) //4-7
				{
					return DDUtilities.MediumSkillColour;
				}

				else if (skillAverage < 0.6f) //8-11
				{
					return DDUtilities.HighSkillColour;
				}

				else if (skillAverage < 0.8f) //12-15
				{
					return DDUtilities.VeryHighSkillColour;
				}

				else //16-20
				{
					return DDUtilities.ExcellentSkillColour;
				}
			}
		}

		public DraggableWorkType(PawnSurface surface)
		{
			this.parent = surface;
		}

		public DraggableWorkType(WorkTypeDef workType, PawnSurface surface = null, bool isPrimary = false, Vector2 posVector = default(Vector2))
		{
			this.def = workType;
			this.parent = surface;
			this.primary = isPrimary;
			this.position = posVector;
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref this.def, "def");
		}
	}
}
