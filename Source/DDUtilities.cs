using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	[StaticConstructorOnStartup]
	public static class DDUtilities
	{
		public const float DraggableTextureWidth = 30f;

		public const float DraggableTextureHeight = 30f;

		public static Vector2 DraggableSize = new Vector2(DraggableTextureWidth, DraggableTextureHeight);

		public static Texture2D ButtonTexture_DisableAll;

		public static Texture2D ButtonTexture_ResetToVanilla;

		static DDUtilities()
		{
			ButtonTexture_DisableAll = ContentFinder<Texture2D>.Get("ButtonDisableAll", true);

			ButtonTexture_ResetToVanilla = ContentFinder<Texture2D>.Get("ButtonResetToDefaults", true);
		}

		//Get the current scrollPosition of the DD Work Tab window
		public static Vector2 TabScrollPosition
		{
			get
			{
				return Find.WindowStack.WindowOfType<MainTabWindow_Work_DragAndDrop>().ScrollPosition;
			}
		}

		//Provides a texture from the DD WorkTab Textures folder for a given WorkTypeDef
		public static Texture2D TextureFromModFolder(WorkTypeDef def)
		{
			Texture2D textureFromModFolder = ContentFinder<Texture2D>.Get("WorkTypeIcons/" + def.defName);

			if (textureFromModFolder != null)
			{
				return textureFromModFolder; //Returns a texture for a specific WorkType
			}

			else return ContentFinder<Texture2D>.Get("WorkTypeIcons/GenericTypeTex"); //Returns a generic WorkType texture
		}

		//True if the user left clicked inside the given Rect
		public static bool MouseLeftClickedRect(Rect rect)
		{
			return Event.current.type == EventType.MouseDown
				&& Event.current.button == 0
				&& Mouse.IsOver(rect);
		}

		//Provides a Rect whose center point is Vector2 "position"
		public static Rect RectOnVector(Vector2 position, Vector2 size)
		{
			float halfWidth = size.x / 2f;
			float halfHeight = size.y / 2f;

			return new Rect(position.x - halfWidth, position.y - halfWidth, size.x, size.y);
		}

		public static string GetLabelForPawn(Pawn pawn)
		{
			//RimWorld.PawnColumnWorker_Label.DoCell
			string nameAdjusted;

			if (!pawn.RaceProps.Humanlike && pawn.Name != null && !pawn.Name.Numerical)
			{
				nameAdjusted = pawn.Name.ToStringShort.CapitalizeFirst() + ", " + pawn.KindLabel;
			}

			else
			{
				nameAdjusted = pawn.LabelCap;
			}

			return nameAdjusted;
		}		
	}
}
