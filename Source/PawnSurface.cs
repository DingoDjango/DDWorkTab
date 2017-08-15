using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace DD_WorkTab
{
	public class PawnSurface : IExposable
	{
		private const float draggableTextureDiameter = DD_Widgets.DraggableTextureDiameter;

		private const float spaceForPawnLabel = DD_Widgets.SpaceForPawnLabel;

		private const float spaceForWorkButtons = DD_Widgets.SpaceForWorkButtons;

		private const float standardSpacing = DD_Widgets.StandardSpacing;

		private const float standardRowHeight = DD_Widgets.StandardRowHeight;

		private static readonly float standardSurfaceWidth = DD_Widgets.PawnSurfaceWidth;

		private List<DraggableWorkType> children = new List<DraggableWorkType>();

		public Dictionary<WorkTypeDef, DraggableWorkType> QuickFindByDef = new Dictionary<WorkTypeDef, DraggableWorkType>();

		public Pawn pawn;

		public List<DraggableWorkType> DraggablesList => this.children;

		private void SortChildrenByPosition()
		{
			this.children = this.children.OrderBy(child => child.position.x).ToList();
		}

		private void ResetChildrenByVanillaDefaults()
		{
			this.children.Clear();

			this.QuickFindByDef.Clear();

			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				if (!this.pawn.story.WorkTypeIsDisabled(def))
				{
					DraggableWorkType newChild = new DraggableWorkType(def, this, false);

					this.children.Add(newChild);
				}
			}
		}

		private void AddOrUpdateChild(DraggableWorkType nomad)
		{
			//Handle primary (undisposable) work types dragged to the surface
			if (nomad.primary)
			{
				//Do not accept disabled work types
				if (this.pawn.story.WorkTypeIsDisabled(nomad.def))
				{
					string forbiddenTypeString = "DD_WorkTab_PawnSurface_WorkTypeForbidden".CachedTranslation(new string[] { nomad.def.gerundLabel }).AdjustedFor(this.pawn);

					Messages.Message(forbiddenTypeString, MessageSound.Silent);

					if (DD_Settings.UseSounds)
					{
						SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
					}

					return;
				}

				//Pawn already has a priority for this work type
				if (this.QuickFindByDef.TryGetValue(nomad.def, out DraggableWorkType existingDraggable))
				{
					existingDraggable.position = nomad.position;
				}

				//Pawn accepts the work type and it was not on the list
				else
				{
					DraggableWorkType newDraggable = new DraggableWorkType(nomad.def, this, false, nomad.position);

					this.children.Add(newDraggable);
				}
			}

			this.SortChildrenByPosition();

			this.UpdatePawnPriorities();

			if (DD_Settings.UseSounds)
			{
				SoundDefOf.MessageBenefit.PlayOneShotOnCamera(null);
			}

			DragManager.CurrentDraggable = null;
		}

		private void RemoveChild(DraggableWorkType disposedType)
		{
			this.pawn.workSettings.Disable(disposedType.def);

			this.QuickFindByDef.Remove(disposedType.def);

			this.children.Remove(disposedType);

			DragManager.CurrentDraggable = null;

			if (DD_Settings.MessageOnDraggableRemoval)
			{
				string disposedTypeString = "DD_WorkTab_PawnSurface_WorkRemoved".CachedTranslation(new string[] { disposedType.def.gerundLabel }).AdjustedFor(this.pawn);

				Messages.Message(disposedTypeString, MessageSound.Silent);
			}

			if (DD_Settings.UseSounds)
			{
				SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
			}
		}

		private void UpdatePawnPriorities()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.pawn.workSettings.SetPriority(this.children[i].def, i + 1);
			}
		}

		private void DrawDynamicPosition(Rect listRect, DraggableWorkType nomad, float xVector)
		{
			GUI.color = !this.pawn.story.WorkTypeIsDisabled(nomad.def) ? Color.white : Color.red;
			float xPosition = listRect.xMin + (standardSpacing / 2f) - 1f;
			float yPosition = listRect.yMin + (standardSpacing / 2f);

			if (this.children.Count > 0)
			{
				for (int i = this.children.Count - 1; i >= 0; i--)
				{
					DraggableWorkType child = this.children[i];

					if (child.position.x < xVector)
					{
						xPosition = child.position.x + (draggableTextureDiameter / 2f) + (standardSpacing / 2f) - 1f;
						yPosition = child.position.y - (draggableTextureDiameter / 2f) - (standardSpacing / 2f);

						break;
					}
				}
			}

			Rect lineRect = new Rect(xPosition, yPosition, 2f, draggableTextureDiameter + standardSpacing);

			GUI.DrawTexture(lineRect, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		public void InsertFromPrimaryShiftClick(int change, WorkTypeDef def)
		{
			int index = this.children.FindIndex(dr => dr.def == def);

			int absoluteIndex = index + change;

			if (index >= 0 && absoluteIndex >= 0 && absoluteIndex != this.children.Count)
			{
				DraggableWorkType d = this.children[index];

				this.children.Remove(d);

				this.children.Insert(absoluteIndex, d);

				this.UpdatePawnPriorities();
			}
		}

		public void DisablePawnWork()
		{
			this.children.Clear();

			this.QuickFindByDef.Clear();

			this.pawn.workSettings.DisableAll();
		}

		public void ResetPawnWorkByDefaults()
		{
			this.ResetChildrenByVanillaDefaults();

			this.UpdatePawnPriorities();
		}

		public void DoWorkTabGUI(Rect surfaceRect, Vector2 scrollOffset, Vector2 mousePosition)
		{
			//Draw draggables, perform drag checks
			Vector2 draggablePositionSetter = new Vector2(surfaceRect.xMin + standardSpacing + (draggableTextureDiameter / 2f), surfaceRect.center.y);

			foreach (DraggableWorkType draggable in this.children)
			{
				if (!draggable.IsDragging)
				{
					draggable.position = draggablePositionSetter;

					draggable.dragRect = draggablePositionSetter.ToDraggableRect();

					DD_Widgets.DrawPassion(this.pawn, draggable.def, draggable.dragRect);
				}

				draggable.DoWorkTabGUI(mousePosition);

				draggablePositionSetter.x += draggableTextureDiameter + standardSpacing;
			}

			//Draw dragging indicator and listen for drop
			if (DragManager.Dragging)
			{
				DraggableWorkType nomad = DragManager.CurrentDraggable;

				if (nomad.parent == this || nomad.primary)
				{
					Vector2 absoluteVector = nomad.primary ? nomad.position + scrollOffset : nomad.position;

					if (surfaceRect.Contains(absoluteVector))
					{
						this.DrawDynamicPosition(surfaceRect, nomad, absoluteVector.x);

						//Draggable dropped onto surface
						if (Event.current.type == EventType.MouseUp)
						{
							this.AddOrUpdateChild(nomad);
						}
					}

					else if (Event.current.type == EventType.MouseUp && nomad.parent == this)
					{
						//Child was dropped outside surface
						this.RemoveChild(nomad);
					}
				}
			}
		}

		public PawnSurface() { }

		public PawnSurface(Pawn pawn)
		{
			this.pawn = pawn;

			this.ResetPawnWorkByDefaults();
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref this.pawn, "pawn", false);

			Scribe_Collections.Look(ref this.children, "children", LookMode.Deep, new object[1] { this });
		}
	}
}
