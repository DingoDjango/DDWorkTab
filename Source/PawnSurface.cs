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
		private const float standardSpacing = DDUtilities.standardSpacing;

		private const float spaceForPawnLabel = DDUtilities.spaceForPawnLabel;

		private const float spaceForWorkButtons = DDUtilities.spaceForWorkButtons;

		private const float draggableWidth = DDUtilities.DraggableTextureWidth;

		private const float draggableHeight = DDUtilities.DraggableTextureHeight;

		private static float surfaceWidth = DDUtilities.standardSurfaceWidth;

		public Pawn pawn;

		private List<DraggableWorkType> children = new List<DraggableWorkType>();

		public List<DraggableWorkType> childrenListForReading
		{
			get
			{
				return this.children;
			}
		}

		private void SortChildrenByPosition()
		{
			this.children = this.children.OrderBy(child => child.position.x).ToList();
		}

		private void AddOrUpdateChild(DraggableWorkType nomad)
		{
			//Handle primary (undisposable) work types dragged to the surface
			if (nomad.primary)
			{
				//Do not accept disabled work types
				if (this.pawn.story.WorkTypeIsDisabled(nomad.def))
				{
					string forbiddenTypeString = "DD_WorkTab_PawnSurface_WorkTypeForbidden".TranslateFast(new string[] { nomad.def.gerundLabel }).AdjustedFor(this.pawn);

					Messages.Message(forbiddenTypeString, MessageSound.Silent);

					if (Settings.UseSounds)
					{
						SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
					}

					return;
				}

				DraggableWorkType existingChild = this.children.Find(child => child.def == nomad.def);

				//Pawn already has a priority for this work type
				if (existingChild != null)
				{
					existingChild.position = nomad.position;
				}

				//Pawn accepts the work type and it was not on the list
				else
				{
					DraggableWorkType newDraggable = new DraggableWorkType(nomad.def, this, false, nomad.position);

					this.children.Add(newDraggable);
				}
			}

			/* Update priority indexes and set priority for all work types
			 * This will also update existing children's priorities if they were dragged around */
			this.SortChildrenByPosition();

			this.RefreshPawnPriorities();

			if (Settings.UseSounds)
			{
				SoundDefOf.MessageBenefit.PlayOneShotOnCamera(null);
			}

			Dragger.CurrentDraggable = null;
		}

		public void RemoveChild(DraggableWorkType typeToRemove)
		{
			if (this.children.Contains(typeToRemove))
			{
				this.pawn.workSettings.Disable(typeToRemove.def);

				if (Settings.MessageOnDraggableRemoval)
				{
					string draggableRemovedString = "DD_WorkTab_PawnSurface_WorkRemoved".TranslateFast(new string[] { typeToRemove.def.gerundLabel }).AdjustedFor(this.pawn);

					Messages.Message(draggableRemovedString, MessageSound.Silent);
				}

				if (Settings.UseSounds)
				{
					SoundDefOf.ClickReject.PlayOneShotOnCamera(null);
				}

				this.children.Remove(typeToRemove);

				typeToRemove = null;
			}

			else
			{
				Log.Error("DDWorkTab :: Attempted to remove DraggableWorkType that was not on the list.");
			}

			Dragger.CurrentDraggable = null;
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

			foreach (WorkTypeDef def in WorkTypeDefsUtility.WorkTypeDefsInPriorityOrder)
			{
				if (this.pawn != null && !this.pawn.story.WorkTypeIsDisabled(def))
				{
					DraggableWorkType newChild = new DraggableWorkType(def, this);

					this.children.Add(newChild);
				}
			}
		}

		//Update pawn priorities (should be called last on drop to this surface)
		public void RefreshPawnPriorities()
		{
			Pawn_WorkSettings workSettings = this.pawn.workSettings;

			if (workSettings != null)
			{
				foreach (WorkTypeDef workType in DefDatabase<WorkTypeDef>.AllDefs)
				{
					int workPriority = this.children.FindIndex(d => d.def == workType) + 1;

					if (workPriority > 0)
					{
						workSettings.SetPriority(workType, workPriority);
					}

					else
					{
						workSettings.SetPriority(workType, 0);
					}
				}
			}
		}

		//Disable all work and re-enable available work types by their vanilla importance
		public void ResetToVanillaSettings()
		{
			this.ResetChildrenByVanillaPriorities();

			this.RefreshPawnPriorities();
		}

		public void OnWorkTabGUI(Rect listRect, Vector2 scrollOffset)
		{
			//Rects
			Rect pawnLabelRect = new Rect(listRect.x, listRect.y, spaceForPawnLabel, listRect.height);
			Rect disableAllRect = new Rect(pawnLabelRect.xMax + standardSpacing, listRect.y + standardSpacing, draggableWidth, draggableHeight);
			Rect resetToVanillaRect = new Rect(disableAllRect.xMax + standardSpacing, disableAllRect.y, disableAllRect.width, disableAllRect.height);
			Rect draggablesRect = new Rect(pawnLabelRect.xMax + spaceForWorkButtons, listRect.y, surfaceWidth, listRect.height);

			//Pawn name
			DDUtilities.DoPawnLabel(pawnLabelRect, pawn);

			//Disable All Work button
			DDUtilities.Button_DisableAllWork(false, this, disableAllRect);

			//Reset to Vanilla button
			DDUtilities.Button_ResetWorkToVanilla(false, this, resetToVanillaRect);

			//Draw draggables, perform drag checks
			Vector2 draggablePositionSetter = new Vector2(draggablesRect.xMin + standardSpacing + (draggableWidth / 2f), listRect.center.y);

			for (int i = 0; i < this.children.Count; i++)
			{
				DraggableWorkType draggable = this.children[i];

				if (!draggable.IsDragging)
				{
					draggable.position = draggablePositionSetter;
				}

				draggable.OnWorkTabGUI();

				draggablePositionSetter.x += draggableWidth + standardSpacing;
			}

			//Draw dragging indicator and listen for drop
			if (Dragger.Dragging)
			{
				DraggableWorkType nomad = Dragger.CurrentDraggable;

				if (nomad.parent == this || nomad.primary)
				{
					Vector2 absoluteVector = nomad.primary ? nomad.position + scrollOffset : nomad.position;

					if (draggablesRect.Contains(absoluteVector))
					{

						this.DrawDynamicPosition(draggablesRect, nomad, absoluteVector);

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

		private void DrawDynamicPosition(Rect listRect, DraggableWorkType nomad, Vector2 absolutePosition)
		{
			GUI.color = !this.pawn.story.WorkTypeIsDisabled(nomad.def) ? Color.white : Color.red;
			Vector2 lineVector = new Vector2(listRect.xMin + (standardSpacing / 2f) - 1f, listRect.yMin + (standardSpacing / 2f));

			if (this.children.Count > 0)
			{
				for (int i = this.children.Count - 1; i >= 0; i--)
				{
					DraggableWorkType child = this.children[i];

					if (child.position.x < absolutePosition.x)
					{
						lineVector.x = child.position.x + (draggableWidth / 2f) + (standardSpacing / 2f) - 1f;
						lineVector.y = child.position.y - (draggableHeight / 2f) - (standardSpacing / 2f);

						break;
					}
				}
			}

			Rect lineRect = new Rect(lineVector.x, lineVector.y, 2f, draggableHeight + standardSpacing);

			GUI.DrawTexture(lineRect, BaseContent.WhiteTex);

			GUI.color = Color.white; //Reset
		}

		public PawnSurface()
		{
		}

		public PawnSurface(Pawn pawn)
		{
			this.pawn = pawn;

			this.ResetChildrenByVanillaPriorities();

			this.RefreshPawnPriorities();
		}

		public void ExposeData()
		{
			Scribe_References.Look(ref this.pawn, "pawn", false);

			Scribe_Collections.Look(ref this.children, "children", LookMode.Deep, new object[1] { this });
		}
	}
}
