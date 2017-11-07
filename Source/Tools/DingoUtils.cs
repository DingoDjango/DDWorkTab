using System.IO;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Tools
{
	public class DingoUtils
	{
		/// <summary>
		/// The alphanumeric name of the mod folder, as defined in RimWorld/Mods.
		/// </summary>
		private string ModFolderName = "DD WorkTab";

		/// <summary>
		/// The numeric folder name of the mod as a Workshop release, as defined in steamapps/workshop/content/294100.
		/// </summary>
		private string WorkshopFolderName = "111111";

		/// <summary>
		/// Generates a high quality texture from a PNG file.
		/// </summary>
		public Texture2D GetHQTexture(string fileName, string folderName = null)
		{
			Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false); //Mipmaps off;

			ModContentPack content = null;

			foreach (ModContentPack mod in LoadedModManager.RunningMods)
			{
				if (mod.Identifier == this.ModFolderName || mod.Identifier == this.WorkshopFolderName)
				{
					content = mod;
				}
			}

			if (content == null)
			{
				Log.Error("Could not find specific mod :: " + this.ModFolderName);

				return texture;
			}

			string texturesPath = Path.Combine(content.RootDir, GenFilePaths.ContentPath<Texture2D>());

			string folderPath = folderName == null ? texturesPath : Path.Combine(new DirectoryInfo(texturesPath).ToString(), folderName);

			DirectoryInfo contentDirectory = new DirectoryInfo(folderPath);

			if (!contentDirectory.Exists)
			{
				Log.Error("Could not find specific textures folder");

				return texture;
			}

			FileInfo image = null;

			foreach (FileInfo f in contentDirectory.GetFiles("*", SearchOption.AllDirectories))
			{
				if (Path.GetFileNameWithoutExtension(f.Name) == fileName)
				{
					image = f;
				}
			}

			if (image == null)
			{
				Log.Error("Could not find specific file name :: " + fileName);

				return texture;
			}

			byte[] fileData = File.ReadAllBytes(image.FullName);

			texture.LoadImage(fileData); //Loads PNG data, sets format to ARGB32 and resizes texture by the source image size
			texture.name = "DD_" + Path.GetFileNameWithoutExtension(fileName);
			texture.filterMode = FilterMode.Trilinear;
			texture.anisoLevel = 9; //default 2, max 9

			/* texture.Compress(true); //Compresses texture
			 * texture.Apply(false, false); //Saves the compressed texture */

			if (texture.width == 2)
			{
				Log.Error("Could not load high quality texture :: " + fileName);
			}

			return texture;
		}

		/// <summary>
		/// Determines which list indexes to render when using a Unity scroll view and fixed item height.
		/// </summary>
		public void VisibleScrollviewIndexes(float scrolledY, float outRectHeight, float itemHeight, int totalItems, out int FirstRenderedIndex, out int LastRenderedIndex)
		{
			int totalRenderedIndexes = (int)(outRectHeight / itemHeight) + 2; //Account for partly rendered surfaces on top/bottom of the Rect

			FirstRenderedIndex = (int)(scrolledY / itemHeight); //Get the first list item that should be at least partly visible

			LastRenderedIndex = Mathf.Min(FirstRenderedIndex + totalRenderedIndexes, totalItems); //Get the last item to render, don't go over list.Count
		}
	}
}
