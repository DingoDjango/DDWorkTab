using System;
using System.Collections.Generic;
using System.Reflection;
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
				return this.children.OrderBy((DraggableWorkType child) => child.priorityIndex);
			}
		}

		public void AddOrUpdateChild(DraggableWorkType typeToAdd, bool isPrimary = false)
		{
			var existingChild = this.children.Find(child => child.def == typeToAdd.def);

			if (existingChild != null)
			{
				existingChild.position = typeToAdd.position;
				typeToAdd = null; //Garbage collection I guess?
			}

			else
			{
				if (this.attachedPawn != null && this.attachedPawn.story.WorkTypeIsDisabled(typeToAdd.def))
				{
					return;
				}

				if (isPrimary)
				{
					var newDraggable = new DraggableWorkType();
					newDraggable.parent = this;
					newDraggable.def = typeToAdd.def;
					newDraggable.position = typeToAdd.position;

					this.children.Add(newDraggable);
				}

				else
				{
					typeToAdd.parent = this;
					this.children.Add(typeToAdd);
				}
			}

			this.UpdateChildIndicesByPosition();
		}

		public void UpdateChildIndicesByPosition()
		{
			IEnumerable<DraggableWorkType> childrenByVectorLeftToRight = this.children.OrderBy((DraggableWorkType child) => child.position.x);

			int priorityByVector = 1;
			foreach (var child in childrenByVectorLeftToRight)
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

		public void DisableAllWork()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i] = null;
			}

			this.children = new List<DraggableWorkType>();

			this.attachedPawn.workSettings.DisableAll();
		}

		public void ResetToVanillaSettings()
		{
			for (int i = 0; i < this.children.Count; i++)
			{
				this.children[i] = null;
			}

			this.children = new List<DraggableWorkType>();

			int priority = 1;

			foreach (var workType in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				if (!this.attachedPawn.story.WorkTypeIsDisabled(workType))
				{
					DraggableWorkType newWorkTypeIndex = new DraggableWorkType(this, workType, priority);
					this.children.Add(newWorkTypeIndex);

					priority++;
				}
			}

			DDUtilities.RefreshPawnPriorities(this.attachedPawn);
		}

		public void OnGUI()
		{
			var dragHelper = Current.Game.GetComponent<DragHelper>();

			float workTypeTextureWidth = DDUtilities.WorkTypeTextureSize.x;

			Vector2 draggablePositionSetter = new Vector2(this.currentListRect.x + 10f + (workTypeTextureWidth / 2f), this.currentListRect.center.y);

			foreach (var draggable in this.childrenSortedByPriority)
			{
				if (!draggable.IsDragging)
				{
					draggable.position = draggablePositionSetter;
				}

				draggable.OnGUI();

				draggablePositionSetter.x += workTypeTextureWidth + 10f;
			}

			if (dragHelper.CurrentDraggingObj.Count > 0)
			{
				var curDraggingObj = dragHelper.CurrentDraggingObj[0];
				Vector2 objectAbsolutePosition = curDraggingObj.position;
				if (curDraggingObj.isPrimaryType)
				{
					objectAbsolutePosition += Find.WindowStack.WindowOfType<MainTabWindow_Work_DragAndDrop>().scrollPosition;
				}

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
								foreach (var child in this.childrenSortedByPriority)
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
						//Update draggables if a main type was dropped inside the surface
						if (this.currentListRect.Contains(objectAbsolutePosition))
						{
							this.AddOrUpdateChild(curDraggingObj, true);

							DDUtilities.RefreshPawnPriorities(this.attachedPawn);
							dragHelper.CurrentDraggingObj.Clear();
						}
					}

					else if (curDraggingObj.parent == this)
					{
						//Nullify draggable if it was dropped outside of the surface
						if (!this.currentListRect.Contains(objectAbsolutePosition))
						{
							this.RemoveChild(curDraggingObj);
							dragHelper.CurrentDraggingObj.Clear();
						}

						//Refresh the pawn's priorities if the user re-ordered this surface's children
						else
						{
							this.UpdateChildIndicesByPosition();

							DDUtilities.RefreshPawnPriorities(this.attachedPawn);
							dragHelper.CurrentDraggingObj.Clear();
						}
					}
				}
				#endregion
			}
		}

		public WorkTypeSurface()
		{
		}

		public WorkTypeSurface(Pawn pawn, List<DraggableWorkType> newChildren)
		{
			this.attachedPawn = pawn;
			this.children = newChildren;
		}

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
