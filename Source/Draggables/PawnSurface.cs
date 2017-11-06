using System.Collections.Generic;
using System.Linq;
using DD_WorkTab.Base;
using DD_WorkTab.Miscellaneous;
using DD_WorkTab.Tools;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab.Draggables
{
	public class PawnSurface : IExposable
	{
		private List<DraggableWork> children = new List<DraggableWork>();

		public Dictionary<WorkTypeDef, DraggableWork> childByDef = new Dictionary<WorkTypeDef, DraggableWork>();

		public Pawn pawn;

		private void OnClicked()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork child = this.children[i];

				if (child.dragRect.Contains(Event.current.mousePosition))
				{
					child.OnClicked();

					return;
				}
			}
		}

		private void OnDrop()
		{
			this.OrderChildrenByPosition();

			this.UpdatePawnPriorities();
		}

		private void OnHover()
		{
			for (int h = 0; h < this.children.Count; h++)
			{
				DraggableWork child = this.children[h];

				if (child.dragRect.Contains(Event.current.mousePosition))
				{
					child.OnHover();

					return;
				}
			}
		}

		private void DrawDynamicPosition(Rect listRect, DraggableWork nomad)
		{
			GUI.color = !this.pawn.story.WorkTypeIsDisabled(nomad.Def) ? Color.white : Color.red;
			int childrenCount = this.children.Count;
			float xPosition = 0f;

			if (childrenCount > 0)
			{
				for (int i = childrenCount - 1; i >= 0; i--)
				{
					DraggableWork child = this.children[i];

					if (child != nomad && child.position.x < nomad.position.x)
					{
						xPosition = child.position.x + Utilities.DraggableTextureDiameter / 2f + Utilities.ShortSpacing / 2f - 1f;

						break;
					}
				}
			}

			if (xPosition == 0f)
			{
				xPosition = listRect.xMin + Utilities.ShortSpacing / 2f - 1f;
			}

			Rect lineRect = new Rect(xPosition, listRect.yMin + (Utilities.ShortSpacing / 2f), 2f, Utilities.DraggableTextureDiameter + Utilities.ShortSpacing);

			GUI.DrawTexture(lineRect, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		private void ConformWorkToPrimaries()
		{
			for (int i = 0; i < Controller.GetPrimaries.PrimaryWorkList.Count; i++)
			{
				DraggableWork work = this.childByDef[Controller.GetPrimaries.PrimaryWorkList[i].def];

				work.position.x = i;
			}

			this.OrderChildrenByPosition();

			this.UpdatePawnPriorities();
		}

		private void ConformWorkToList(List<DraggableWork> copiedList)
		{
			for (int i = 0; i < copiedList.Count; i++)
			{
				DraggableWork copiedWork = copiedList[i];
				DraggableWork child = this.childByDef[copiedWork.Def];

				if (!copiedWork.CompletelyDisabled && !child.CompletelyDisabled)
				{
					child.Disabled = copiedWork.Disabled;
				}

				child.position.x = i;
			}

			this.OrderChildrenByPosition();

			this.UpdatePawnPriorities();
		}

		private void UpdatePawnPriorities()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork current = this.children[i];

				if (!current.Disabled)
				{
					this.pawn.workSettings.SetPriority(this.children[i].Def, i + 1);
				}

				else
				{
					this.pawn.workSettings.Disable(current.Def);
				}
			}
		}

		private void OrderChildrenByPosition()
		{
			this.children = this.children.OrderByDescending(child => !child.CompletelyDisabled).ThenBy(child => child.position.x).ToList();
		}

		private void RefreshChildrenDictionary()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork current = this.children[i];

				this.childByDef[current.Def] = current;
			}
		}

		public void DrawSurface(Rect surfaceRect)
		{
			Vector2 positionSetter = new Vector2(surfaceRect.x + 2f * Utilities.ShortSpacing + Utilities.DraggableTextureDiameter / 2f, surfaceRect.center.y);

			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork draggable = this.children[i];

				if (!draggable.DraggingNow)
				{
					draggable.position = positionSetter;

					draggable.dragRect = positionSetter.ToDraggableRect();

					draggable.DrawTexture(draggable.dragRect);
				}

				else if (surfaceRect.Contains(draggable.position))
				{
					this.DrawDynamicPosition(surfaceRect, draggable);
				}

				positionSetter.x += Utilities.DraggableTextureDiameter + Utilities.ShortSpacing;
			}
		}

		public void DoEventChecks(Rect surfaceRect)
		{
			DraggableWork nomad = Controller.CurrentDraggable;

			if (nomad != null)
			{
				if (nomad.parent == this && Event.current.type == EventType.MouseUp)
				{
					nomad.OnDrop();

					if (surfaceRect.Contains(nomad.position))
					{
						this.OnDrop();

						if (Controller.UseSounds)
						{
							if (!nomad.CompletelyDisabled)
							{
								Utilities.TaskCompleted.PlayOneShotOnCamera(null);
							}

							else
							{
								Utilities.TaskFailed.PlayOneShotOnCamera(null);

								if (Controller.VerboseMessages)
								{
									string draggedIncapableWork = "DD_WorkTab_Message_DraggedIncapableWork".CachedTranslation(new string[] { nomad.Def.labelShort }).AdjustedFor(this.pawn);

									Messages.Message(draggedIncapableWork, MessageTypeDefOf.SilentInput);
								}
							}
						}
					}

				}
			}

			else if (surfaceRect.Contains(Event.current.mousePosition))
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
			int index = this.children.FindIndex(dr => dr.Def == def);

			DraggableWork affectedWork = this.children[index];

			int postChange = index + change;

			if (postChange >= 0 && postChange < this.children.Count)
			{
				this.children.Remove(affectedWork);

				this.children.Insert(postChange, affectedWork);

				this.UpdatePawnPriorities();
			}
		}

		public void EnableWorkType(DraggableWork draggable, bool suppressVerbosity = false)
		{
			int index = this.children.FindIndex(d => d == draggable);

			draggable.Disabled = false; //Will stop and notify the user if they're trying to enable a CompletelyDisabled type

			if (!draggable.Disabled) //Check state post-change
			{
				this.pawn.workSettings.SetPriority(draggable.Def, index + 1);

				if (!suppressVerbosity)
				{
					if (Controller.VerboseMessages)
					{
						string enabledWorkText = "DD_WorkTab_Message_WorkEnabled".CachedTranslation(new string[] { draggable.Def.labelShort }).AdjustedFor(this.pawn);

						Messages.Message(enabledWorkText, MessageTypeDefOf.SilentInput);
					}

					if (Controller.UseSounds)
					{
						Utilities.WorkEnabled.PlayOneShotOnCamera(null);
					}
				}
			}
		}

		public void DisableWorkType(DraggableWork draggable, bool suppressVerbosity = false)
		{
			this.pawn.workSettings.Disable(draggable.Def);

			draggable.Disabled = true;

			if (!suppressVerbosity)
			{
				if (Controller.VerboseMessages)
				{
					string disabledWorkText = "DD_WorkTab_Message_WorkDisabled".CachedTranslation(new string[] { draggable.Def.labelShort }).AdjustedFor(this.pawn);

					Messages.Message(disabledWorkText, MessageTypeDefOf.SilentInput);
				}

				if (Controller.UseSounds)
				{
					Utilities.WorkDisabled.PlayOneShotOnCamera(null);
				}
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

		public void ResetWorkToDefaultState()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWork current = this.children[i];

				if (!current.CompletelyDisabled)
				{
					current.Disabled = false;
				}
			}

			this.ConformWorkToPrimaries();
		}

		public void CopyPriorities()
		{
			Controller.CopyPrioritiesReference = this;

			if (Controller.VerboseMessages)
			{
				Messages.Message("DD_WorkTab_Message_CopiedPriorities".CachedTranslation().AdjustedFor(this.pawn), MessageTypeDefOf.SilentInput);
			}

			if (Controller.UseSounds)
			{
				Utilities.TaskCompleted.PlayOneShotOnCamera(null);
			}
		}

		public void PastePriorities(PawnSurface referencedSurface)
		{
			if (referencedSurface == null)
			{
				Messages.Message("DD_WorkTab_Message_NoCopyPasteReference".CachedTranslation(), MessageTypeDefOf.SilentInput);

				Utilities.TaskFailed.PlayOneShotOnCamera(null);

				return;
			}

			this.ConformWorkToList(referencedSurface.children);

			if (Controller.VerboseMessages)
			{
				string message = "DD_WorkTab_Message_PastedPriorities".CachedTranslation(new string[] { referencedSurface.pawn.NameStringShort }).AdjustedFor(this.pawn);

				Messages.Message(message, MessageTypeDefOf.SilentInput);
			}

			if (Controller.UseSounds)
			{
				Utilities.TaskCompleted.PlayOneShotOnCamera(null);
			}
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
