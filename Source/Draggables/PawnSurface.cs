using System.Collections.Generic;
using System.Linq;
using DD_WorkTab.Base;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Primaries;
using DD_WorkTab.Tools;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Draggables
{
	public class PawnSurface : IExposable
	{
		private List<DraggableWork> children = new List<DraggableWork>();

		public Dictionary<WorkTypeDef, DraggableWork> childByDef = new Dictionary<WorkTypeDef, DraggableWork>();

		public Pawn pawn;

		private DraggableWork ChildOnMousePosition => this.children.Find(c => c.dragRect.Contains(Event.current.mousePosition));

		private void RefreshChildrenDictionary()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork current = this.children[i];

				this.childByDef[current.Def] = current;
			}
		}

		private void OrderChildrenByPosition()
		{
			this.children = this.children.OrderByDescending(child => !child.CompletelyDisabled).ThenBy(child => child.position.x).ToList();
		}

		private void UpdatePawnPriorities()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork current = this.children[i];

				int priority = current.Disabled ? 0 : i + 1;

				this.pawn.workSettings.SetPriority(current.Def, priority);
			}
		}

		private void OrderAndUpdatePriorities()
		{
			this.OrderChildrenByPosition();

			this.UpdatePawnPriorities();
		}

		private void ConformWorkToList(List<DraggableWork> copiedList)
		{
			for (int i = 0; i < copiedList.Count; i++)
			{
				DraggableWork copiedChild = copiedList[i];
				DraggableWork affectedChild = this.childByDef[copiedChild.Def];

				if (!copiedChild.CompletelyDisabled && !affectedChild.CompletelyDisabled)
				{
					affectedChild.Disabled = copiedChild.Disabled;
				}

				affectedChild.position.x = i;
			}

			this.OrderAndUpdatePriorities();
		}

		private void DrawPotentialPosition(Rect rect, DraggableWork nomad)
		{
			DraggableWork child = this.children.FindLast(c => c != nomad && c.position.x < nomad.position.x);

			float xPosition = child != null ? child.dragRect.xMax : rect.xMin;

			Rect lineRect = new Rect(xPosition + Utilities.ShortSpacing / 2f - 1f, rect.yMin + Utilities.ShortSpacing / 2f, 2f, Utilities.DraggableDiameter + Utilities.ShortSpacing);

			GUI.color = !nomad.CompletelyDisabled ? Color.white : Color.red;

			GUI.DrawTexture(lineRect, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		private void OnClicked()
		{
			this.ChildOnMousePosition?.OnClicked();
		}

		private void OnHover()
		{
			DraggableWork child = this.ChildOnMousePosition;

			if (child != null)
			{
				child.OnHover(child.dragRect, false);
			}
		}

		public void DrawSurface(Rect rect)
		{
			Vector2 positionSetter = new Vector2(rect.x + 2f * Utilities.ShortSpacing + Utilities.DraggableDiameter / 2f, rect.center.y);

			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork draggable = this.children[i];

				if (!draggable.DraggingNow)
				{
					draggable.position = positionSetter;

					draggable.dragRect = positionSetter.ToWorkRect();

					draggable.DrawTexture(draggable.dragRect);
				}

				else if (rect.Contains(draggable.position))
				{
					this.DrawPotentialPosition(rect, draggable);
				}

				positionSetter.x += Utilities.DraggableDiameter + Utilities.ShortSpacing;
			}
		}

		public void DoWorkTabEventChecks(Rect rect)
		{
			DraggableWork nomad = Controller.CurrentDraggable;

			if (nomad != null)
			{
				if (Event.current.type == EventType.MouseUp && nomad.parent == this)
				{
					nomad.OnDrop();

					if (rect.Contains(nomad.position))
					{
						this.OrderAndUpdatePriorities();

						string badDrag = !nomad.CompletelyDisabled ? "" : "DD_WorkTab_Message_DraggedIncapableWork".CachedTranslation(new string[] { nomad.Def.labelShort }).AdjustedFor(this.pawn);
						WorkSound sound = !nomad.CompletelyDisabled ? WorkSound.TaskCompleted : WorkSound.TaskFailed;

						Utilities.UserFeedbackChain(sound, badDrag);
					}
				}
			}

			else if (rect.Contains(Event.current.mousePosition))
			{
				if (Event.current.type == EventType.Repaint)
				{
					this.OnHover();
				}

				else if (Event.current.type == EventType.MouseDown)
				{
					this.OnClicked();
				}
			}
		}

		public void OnPrimaryShiftClick(int change, WorkTypeDef def)
		{
			DraggableWork affectedWork = this.childByDef[def];

			int postChange = this.children.FindIndex(c => c == affectedWork) + change;

			int firstIncapableWork = this.children.FindIndex(c => c.CompletelyDisabled);

			if (postChange >= 0 && postChange < this.children.Count)
			{
				if (firstIncapableWork >= 0 && postChange > firstIncapableWork)
				{
					return; //Don't move the draggable past the incapable-of area
				}

				this.children.Remove(affectedWork);

				this.children.Insert(postChange, affectedWork);

				this.UpdatePawnPriorities();
			}
		}

		public void OnPrimaryCtrlClick(bool enable, WorkTypeDef def)
		{
			DraggableWork affectedWork = this.childByDef[def];

			if (!affectedWork.CompletelyDisabled)
			{
				if (enable)
				{
					this.EnableWorkType(affectedWork, true);
				}

				else
				{
					this.DisableWorkType(affectedWork, true);
				}
			}
		}

		public void EnableWorkType(DraggableWork work, bool noFeedback = false)
		{
			work.Disabled = false; //Will stop and notify the user if they're trying to enable a CompletelyDisabled type

			if (!work.Disabled) //Check state post-change
			{
				this.UpdatePawnPriorities();

				if (!noFeedback)
				{
					string workEnabled = "DD_WorkTab_Message_WorkEnabled".CachedTranslation(new string[] { work.Def.labelShort }).AdjustedFor(this.pawn);

					Utilities.UserFeedbackChain(WorkSound.WorkEnabled, workEnabled);
				}
			}
		}

		public void DisableWorkType(DraggableWork work, bool noFeedback = false)
		{
			work.Disabled = true;

			this.UpdatePawnPriorities();

			if (!noFeedback)
			{
				string workDisabled = "DD_WorkTab_Message_WorkDisabled".CachedTranslation(new string[] { work.Def.labelShort }).AdjustedFor(this.pawn);

				Utilities.UserFeedbackChain(WorkSound.WorkDisabled, workDisabled);
			}
		}

		public void EnableAllPawnWork()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork child = this.children[i];

				if (!child.CompletelyDisabled)
				{
					child.Disabled = false;
				}
			}

			this.UpdatePawnPriorities();
		}

		public void DisableAllPawnWork()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].Disabled = true;
			}

			this.UpdatePawnPriorities();
		}

		public void ResetAllPawnWork()
		{
			List<PrimaryWork> primaries = Controller.GetPrimaries.PrimaryWorkList;

			for (int i = 0; i < primaries.Count; i++)
			{
				DraggableWork current = this.childByDef[primaries[i].def];

				if (!current.CompletelyDisabled)
				{
					current.Disabled = false;
				}

				current.position.x = i;
			}

			this.OrderAndUpdatePriorities();
		}

		public void CopyPriorities()
		{
			Controller.CopyPrioritiesReference = this;

			string copiedPriorities = "DD_WorkTab_Message_CopiedPriorities".CachedTranslation().AdjustedFor(this.pawn);

			Utilities.UserFeedbackChain(WorkSound.TaskCompleted, copiedPriorities);
		}

		public void PastePriorities(PawnSurface referencedSurface)
		{
			if (referencedSurface == null)
			{
				string noReference = "DD_WorkTab_Message_NoCopyPasteReference".CachedTranslation();

				Utilities.UserFeedbackChain(WorkSound.TaskFailed, noReference);

				return;
			}

			this.ConformWorkToList(referencedSurface.children);

			string pastedPriorities = "DD_WorkTab_Message_PastedPriorities".CachedTranslation(new string[] { referencedSurface.pawn.NameStringShort }).AdjustedFor(this.pawn);

			Utilities.UserFeedbackChain(WorkSound.TaskCompleted, pastedPriorities);
		}

		public void RecacheDraggableOutlines()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i].RecacheOutline();
			}
		}

		public PawnSurface()
		{
		}

		public PawnSurface(Pawn pawn)
		{
			this.pawn = pawn;

			Pawn_WorkSettings workSettings = this.pawn.workSettings;

			for (int i = 0; i < Controller.GetPrimaries.PrimaryWorkList.Count; i++)
			{
				WorkTypeDef def = Controller.GetPrimaries.PrimaryWorkList[i].def;
				bool incapablePawn = this.pawn.story.WorkTypeIsDisabled(def);
				bool currentlyDisabled = workSettings.GetPriority(def) == 0; //Account for pre-existing priorities

				DraggableWork newWork = new DraggableWork(def, incapablePawn, this, currentlyDisabled);

				this.children.Add(newWork);
			}

			this.RefreshChildrenDictionary();

			this.children = this.children.OrderByDescending(d => !d.CompletelyDisabled).ThenByDescending(d => workSettings.GetPriority(d.Def) != 0).ThenBy(d => workSettings.GetPriority(d.Def)).ToList(); //Account for pre-existing priorities

			this.UpdatePawnPriorities();
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref this.pawn, "pawn", false);

			Scribe_Collections.Look(ref this.children, "children", LookMode.Deep, new object[1] { this });

			if (Scribe.mode == LoadSaveMode.LoadingVars)
			{
				this.RefreshChildrenDictionary();
			}
		}
	}
}
