using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class PawnSurface : IExposable
	{
		public Pawn pawn;

		private List<DraggableWorkType> children = new List<DraggableWorkType>();

		private float standardSpacing = MainTabWindow_Work_DragAndDrop.spaceBetweenTypes;

		private DragManager dragger
		{
			get
			{
				return Current.Game.World.GetComponent<DragManager>();
			}
		}

		public List<DraggableWorkType> childrenListForReading //Unused
		{
			get
			{
				return this.children;
			}
		}

		public IEnumerable<DraggableWorkType> childrenSortedByPriority
		{
			get
			{
				return this.children.OrderBy(child => child.priorityIndex);
			}
		}

		public void AddOrUpdateChild(DraggableWorkType typeToAdd, bool isPrimary)
		{
			//Do not accept disabled work types
			if (this.pawn.story.WorkTypeIsDisabled(typeToAdd.def))
			{
				string text = string.Format("Cannot assign {0} to {1}. {2} is incapable of it.", new object[]
					{ /* change Format to Translate later */
						this.pawn.NameStringShort,
						typeToAdd.def.labelShort,
						this.pawn.ProSubjCap()
					});
				Messages.Message(text, MessageSound.RejectInput);

				return;
			}

			//Handle primary (undisposable) work types dragged to the surface
			if (isPrimary)
			{
				DraggableWorkType existingChild = this.children.Find(child => child.def == typeToAdd.def);

				//Pawn already has a priority for this work type
				if (existingChild != null)
				{
					existingChild.position = typeToAdd.position;
				}

				//Pawn accepts the work type and it was not on the list
				else
				{
					DraggableWorkType newDraggable = new DraggableWorkType(this, typeToAdd.def, typeToAdd.position);

					this.children.Add(newDraggable);
				}
			}

			/* Update priority indexes and set priority for all work types
			 * This will also update existing children's priorities if they were dragged around */
			this.UpdateChildIndicesByPosition();

			this.RefreshPawnPriorities();
		}

		public void UpdateChildIndicesByPosition()
		{
			IEnumerable<DraggableWorkType> childrenByVectorLeftToRight = this.children.OrderBy(child => child.position.x);

			int priorityByVector = 1;

			foreach (DraggableWorkType child in childrenByVectorLeftToRight)
			{
				child.priorityIndex = priorityByVector;

				priorityByVector++;
			}
		}

		public void RemoveChild(DraggableWorkType typeToRemove)
		{
			if (this.children.Contains(typeToRemove))
			{
				this.pawn.workSettings.Disable(typeToRemove.def);

				this.children.Remove(typeToRemove);

				typeToRemove = null;
			}

			else
			{
				Log.Error("DDWorkTab :: Attempted to remove WorkTypeIndex that was not on the list.");
			}
		}

		//Clear children and set all work types to priority 0 (disabled)
		public void DisableAllWork()
		{
			if (this.children.Count > 0)
			{
				for (int i = this.children.Count - 1; i >= 0; i--)
				{
					this.children[i] = null;
				}
			}

			this.children.Clear();

			this.pawn.workSettings.DisableAll();
		}

		//Populate children list with all viable work types
		public void ResetChildrenByVanillaPriorities()
		{
			this.children.Clear();

			int priority = 1;

			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				if (this.pawn != null && !this.pawn.story.WorkTypeIsDisabled(def))
				{
					DraggableWorkType newChild = new DraggableWorkType(this, def, priority);

					this.children.Add(newChild);

					priority++;
				}
			}
		}

		//Update pawn priorities (should be called last on drop to this surface)
		public void RefreshPawnPriorities()
		{
			Pawn_WorkSettings workSettings = this.pawn.workSettings;

			if (workSettings == null)
			{
				workSettings = new Pawn_WorkSettings(this.pawn);
			}

			workSettings.DisableAll();

			foreach (DraggableWorkType draggable in this.childrenSortedByPriority)
			{
				workSettings.SetPriority(draggable.def, draggable.priorityIndex);
			}
		}

		//Disable all work and re-enable available work types by their vanilla importance
		public void ResetToVanillaSettings()
		{
			this.DisableAllWork();

			this.ResetChildrenByVanillaPriorities();

			this.RefreshPawnPriorities();
		}

		public void OnGUI(Rect listRect)
		{
			//Draw draggables, perform drag checks
			if (this.children.Count > 0)
			{
				Vector2 draggablePositionSetter = new Vector2(listRect.x + this.standardSpacing + (DDUtilities.DraggableTextureWidth / 2f), listRect.center.y);

				foreach (DraggableWorkType draggable in this.childrenSortedByPriority)
				{
					if (!draggable.IsDragging)
					{
						draggable.position = draggablePositionSetter;
					}

					draggable.OnGUI();

					if (!dragger.Dragging)
					{
						draggable.DrawStationaryInformation(DDUtilities.RectOnVector(draggablePositionSetter, DDUtilities.DraggableSize), false);
					}

					draggablePositionSetter.x += DDUtilities.DraggableTextureWidth + this.standardSpacing;
				}
			}

			//Draw dragging indicator and listen for drop
			if (dragger.Dragging)
			{
				DraggableWorkType curDraggingObj = dragger.CurrentDraggingObj[0];

				Vector2 absPos = curDraggingObj.isPrimaryType ? curDraggingObj.position + DDUtilities.TabScrollPosition : curDraggingObj.position;

				if (curDraggingObj.parent == this || curDraggingObj.isPrimaryType)
				{
					if (listRect.Contains(absPos))
					{
						this.DrawDynamicPosition(listRect, curDraggingObj, absPos);
					}
				}

				if (Event.current.type == EventType.MouseUp)
				{
					this.ConsiderPlacementOnMouseUp(listRect, curDraggingObj, absPos);
				}
			}
		}

		private void DrawDynamicPosition(Rect listRect, DraggableWorkType nomad, Vector2 absolutePosition)
		{
			GUI.color = Color.white;
			Vector2 lineVector = Vector2.zero;

			//Consider existing draggables' positions
			if (this.children.Count > 0)
			{
				foreach (DraggableWorkType child in this.childrenSortedByPriority.Reverse())
				{
					if (child.position.x < absolutePosition.x)
					{
						lineVector.x = child.position.x + (DDUtilities.DraggableTextureWidth / 2f) + (this.standardSpacing / 2f);
						lineVector.y = child.position.y - (DDUtilities.DraggableTextureHeight / 2f) - (this.standardSpacing / 2f);
						break;
					}

					lineVector.x = listRect.xMin + (this.standardSpacing / 2f);
					lineVector.y = listRect.yMin + (this.standardSpacing / 2f);
				}
			}

			//The pawn surface was vacant
			else
			{
				lineVector.x = listRect.xMin + (this.standardSpacing / 2f);
				lineVector.y = listRect.yMin + (this.standardSpacing / 2f);
			}

			if (this.pawn.story.WorkTypeIsDisabled(nomad.def))
			{
				GUI.color = Color.red; //Indicates incompatible work type

				Widgets.DrawLineVertical(lineVector.x, lineVector.y, DDUtilities.DraggableTextureHeight + this.standardSpacing);

				GUI.color = Color.white; //Reset
			}

			else
			{
				Widgets.DrawLineVertical(lineVector.x, lineVector.y, DDUtilities.DraggableTextureHeight + this.standardSpacing);
			}
		}

		private void ConsiderPlacementOnMouseUp(Rect listRect, DraggableWorkType nomad, Vector2 absolutePosition)
		{
			//Primary draggable dropped onto this surface
			if (nomad.isPrimaryType && listRect.Contains(absolutePosition))
			{
				this.AddOrUpdateChild(nomad, true);

				dragger.CurrentDraggingObj.Clear();
			}

			//Draggable was dragged from within this surface
			else if (nomad.parent == this)
			{
				//Draggable dropped outside of surface (the player wants to disable this work type)
				if (!listRect.Contains(absolutePosition))
				{
					this.RemoveChild(nomad);
				}

				//Reconsider work type priorities (total count unchanged)
				else
				{
					this.AddOrUpdateChild(nomad, false);
				}

				dragger.CurrentDraggingObj.Clear();
			}
		}

		public PawnSurface()
		{
		}

		public PawnSurface(Pawn pawn)
		{
			this.pawn = pawn;

			this.ResetChildrenByVanillaPriorities();
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref this.pawn, "pawn", false);

			Scribe_Collections.Look(ref this.children, "children", LookMode.Deep, new object[1]
			{
				this
			});
		}
	}
}
