using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public class WorkTypeSurface : IExposable
	{
		public Pawn attachedPawn = null;

		private List<DraggableWorkType> children = new List<DraggableWorkType>();

		public Rect currentListRect;

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

		public void AddOrUpdateChild(DraggableWorkType typeToAdd, bool isPrimary = false)
		{
			//Do not accept disabled work types
			if (this.attachedPawn != null && this.attachedPawn.story.WorkTypeIsDisabled(typeToAdd.def))
			{
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
					DraggableWorkType newDraggable = new DraggableWorkType(this, typeToAdd.def, -1, typeToAdd.position);

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
				this.attachedPawn.workSettings.Disable(typeToRemove.def);

				this.children.Remove(typeToRemove);

				typeToRemove = null;
			}

			else
			{
				Log.Error("DDWorkTab :: Attempted to remove WorkTypeIndex that was not in the list.");
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

			this.attachedPawn.workSettings.DisableAll();
		}

		//Populate children list with all viable work types
		public void ResetChildrenByVanillaPriorities()
		{
			this.children.Clear();

			int priority = 1;

			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				if (this.attachedPawn != null && !this.attachedPawn.story.WorkTypeIsDisabled(def))
				{
					DraggableWorkType newWorkTypeIndex = new DraggableWorkType(this, def, priority);

					this.children.Add(newWorkTypeIndex);

					priority++;
				}
			}
		}

		public void RefreshPawnPriorities()
		{
			if (this.attachedPawn != null)
			{
				Pawn_WorkSettings workSettings = this.attachedPawn.workSettings;

				if (workSettings == null)
				{
					workSettings = new Pawn_WorkSettings(this.attachedPawn);

					workSettings.DisableAll();
				}

				foreach (DraggableWorkType draggable in this.childrenSortedByPriority)
				{
					workSettings.SetPriority(draggable.def, draggable.priorityIndex);
				}
			}
		}

		//DisableAllWork + re-enable available work types by their vanilla importance
		public void ResetToVanillaSettings()
		{
			this.DisableAllWork();

			this.ResetChildrenByVanillaPriorities();

			this.RefreshPawnPriorities();
		}

		public void OnGUI()
		{
			DragHelper dragger = Current.Game.GetComponent<DragHelper>();

			float workTypeTextureWidth = DDUtilities.WorkTypeTextureSize.x;

			Vector2 draggablePositionSetter = new Vector2(this.currentListRect.x + 10f + (workTypeTextureWidth / 2f), this.currentListRect.center.y);

			foreach (DraggableWorkType draggable in this.childrenSortedByPriority)
			{
				if (!draggable.IsDragging)
				{
					draggable.position = draggablePositionSetter;
				}

				draggable.OnGUI();

				draggablePositionSetter.x += workTypeTextureWidth + 10f;
			}

			if (dragger.CurrentDraggingObj.Count > 0)
			{
				DraggableWorkType curDraggingObj = dragger.CurrentDraggingObj[0];

				Vector2 objectAbsolutePosition = curDraggingObj.isPrimaryType ? curDraggingObj.position + Find.WindowStack.WindowOfType<MainTabWindow_Work_DragAndDrop>().scrollPosition : curDraggingObj.position;

				#region Draw Line While Dragging
				if (curDraggingObj.parent == this || curDraggingObj.isPrimaryType)
				{
					if (this.currentListRect.Contains(objectAbsolutePosition))
					{
						GUI.color = Color.white;
						Vector2 lineVector = Vector2.zero;

						if (this.children.Count > 0)
						{
							if (curDraggingObj != this.childrenSortedByPriority.Last() && objectAbsolutePosition.x > this.childrenSortedByPriority.Last().position.x)
							{
								lineVector.x = this.childrenSortedByPriority.Last().position.x + (DDUtilities.WorkTypeTextureSize.x / 2f) + 5f;
								lineVector.y = this.childrenSortedByPriority.Last().position.y - (DDUtilities.WorkTypeTextureSize.y / 2f) - 5f;
							}

							else
							{
								foreach (DraggableWorkType child in this.childrenSortedByPriority)
								{
									if (child.position.x > objectAbsolutePosition.x)
									{
										lineVector.x = child.position.x - (DDUtilities.WorkTypeTextureSize.x / 2f) - 5f;
										lineVector.y = child.position.y - (DDUtilities.WorkTypeTextureSize.y / 2f) - 5f;
										break;
									}
								}
							}
						}

						else
						{
							lineVector.x = this.currentListRect.xMin + 5f;
							lineVector.y = this.currentListRect.yMin + 5f;
						}

						if (lineVector != Vector2.zero)
						{
							Widgets.DrawLineVertical(lineVector.x, lineVector.y, DDUtilities.WorkTypeTextureSize.y + 10f);
						}
					}
				}
				#endregion

				#region Place Draggable
				if (Event.current.type == EventType.MouseUp)
				{
					if (curDraggingObj.isPrimaryType)
					{
						//Update children (main type dropped into surface)
						if (this.currentListRect.Contains(objectAbsolutePosition))
						{
							this.AddOrUpdateChild(curDraggingObj, true);

							dragger.CurrentDraggingObj.Clear();
						}
					}

					else if (curDraggingObj.parent == this)
					{
						//Disable work type and nullify draggable if it was dropped outside of the surface
						if (!this.currentListRect.Contains(objectAbsolutePosition))
						{
							this.RemoveChild(curDraggingObj);

							dragger.CurrentDraggingObj.Clear();
						}

						//Update children (type priority changed)
						else
						{
							this.AddOrUpdateChild(curDraggingObj, false);

							dragger.CurrentDraggingObj.Clear();
						}
					}
				}
				#endregion
			}
		}

		#region Constructors
		public WorkTypeSurface()
		{
		}

		public WorkTypeSurface(Pawn pawn)
		{
			this.attachedPawn = pawn;
		}

		public WorkTypeSurface(Pawn pawn, List<DraggableWorkType> newChildren)
		{
			this.attachedPawn = pawn;
			this.children = newChildren;
		}
		#endregion

		public void ExposeData()
		{
			if (this.attachedPawn != null)
			{
				Scribe_References.Look(ref this.attachedPawn, "attachedPawn");
			}
			Scribe_Collections.Look(ref this.children, "children", LookMode.Deep, new object[] { this });
		}
	}
}
