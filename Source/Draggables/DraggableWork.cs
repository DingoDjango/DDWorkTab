using DD_WorkTab.Base;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Draggables
{
	/* Based on the work of Emil Johansen aka AngryAnt
	* https://github.com/AngryAnt */

	public class DraggableWork : DraggableObject, IExposable
	{
		private WorkTypeDef def;

		private bool completelyIncapable;

		private bool disabled;

		private WorkTypeInfo workInfo;

		private Color outlineColour;

		public readonly PawnSurface parent;

		public WorkTypeDef Def => this.def;

		public bool CompletelyDisabled => this.completelyIncapable;

		public bool Disabled
		{
			get => this.disabled;

			set
			{
				if (!this.completelyIncapable)
				{
					this.disabled = value;
				}

				else if (value == false)
				{
					string incapablePawn = "DD_WorkTab_Message_IncapablePawn".CachedTranslation(new string[] { this.def.labelShort }).AdjustedFor(this.parent.pawn);

					Utilities.UserFeedbackChain(WorkSound.TaskFailed, incapablePawn);
				}
			}
		}

		private Color ColourFromSkillRange(float skillAverage)
		{
			if (this.def.relevantSkills.Count == 0)
			{
				return Utilities.MediumSkillColour;
			}

			if (skillAverage < 4f) //0-3
			{
				return Utilities.LowSkillColour;
			}

			if (skillAverage < 8f) //4-7
			{
				return Utilities.MediumSkillColour;
			}

			if (skillAverage < 12f) //8-11
			{
				return Utilities.HighSkillColour;
			}

			if (skillAverage < 16f) //12-15
			{
				return Utilities.VeryHighSkillColour;
			}

			return Utilities.ExcellentSkillColour; //16-20
		}

		private Color GetDynamicColour()
		{
			if (this.completelyIncapable || Utilities.CapacitiesCompromisedForWorkType(this.parent.pawn, this.def))
			{
				return Color.red;
			}

			if (this.outlineColour == default(Color))
			{
				this.RecacheOutline();
			}

			return this.outlineColour;
		}

		public void RecacheOutline()
		{
			float skillAverage = this.parent.pawn.skills.AverageOfRelevantSkillsFor(this.def);

			this.outlineColour = this.ColourFromSkillRange(skillAverage);
		}

		public void DrawTexture(Rect drawRect)
		{
			Utilities.DraggableOutline(drawRect, this.GetDynamicColour());

			Rect contractedDrawRect = drawRect.ContractedBy(2f);

			if (!this.disabled)
			{
				GUI.DrawTexture(contractedDrawRect, this.workInfo.texture64); //Normal texture indicates enabled work
			}

			else if (!this.completelyIncapable)
			{
				GUI.DrawTexture(contractedDrawRect, this.workInfo.texture64_Disabled); //Outline texture indicates intentionally disabled work
			}

			else
			{
				GUI.DrawTexture(contractedDrawRect, this.workInfo.texture64_Greyscale); //Greyscale texture indicates incapability

				GUI.DrawTexture(drawRect, Utilities.IncapableWorkerX); //Red X on top of greyscale texture
			}

			Utilities.DrawPassion(drawRect, this.parent.pawn, this.def);
		}

		public override void OnClicked()
		{
			if (!Event.current.control)
			{
				if (Event.current.button == 0)
				{
					this.draggingNow = true;
					this.dragOffsetFromMouse = Event.current.mousePosition - this.position;

					Controller.CurrentDraggable = this;
				}
			}

			else
			{
				if (!this.disabled)
				{
					this.parent.DisableWorkType(this);
				}

				else
				{
					this.parent.EnableWorkType(this);
				}
			}

			Event.current.Use();
		}

		public override void OnDrag()
		{
			this.position = Event.current.mousePosition - this.dragOffsetFromMouse;
		}

		public override void OnDrop()
		{
			this.draggingNow = false;

			Controller.CurrentDraggable = null;
		}

		public override void OnHover()
		{
			Widgets.DrawHighlight(this.dragRect);

			TooltipHandler.TipRegion(this.dragRect, Utilities.DraggableTooltip(this.def, false, false, this.completelyIncapable, this.disabled, this.parent.pawn));
		}

		public DraggableWork(PawnSurface surface)
		{
			this.parent = surface;
		}

		public DraggableWork(WorkTypeDef def, bool incapablePawn, PawnSurface surface, bool disabled)
		{
			this.def = def;
			this.completelyIncapable = incapablePawn;
			this.parent = surface;

			if (incapablePawn || disabled)
			{
				this.disabled = true;
			}

			this.workInfo = Utilities.WorkDefAttributes[this.def];
		}

		public void ExposeData()
		{
			Scribe_Defs.Look(ref this.def, "def");
			Scribe_Values.Look(ref this.completelyIncapable, "completelyIncapable");
			Scribe_Values.Look(ref this.disabled, "disabled");

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.workInfo = Utilities.WorkDefAttributes[this.def];
			}
		}
	}
}
