using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Verse;

namespace DD_WorkTab.Tools
{
	public static class DingoUtils
	{
		/// <summary>
		/// The alphanumeric name of the mod defined in its About.xml file.
		/// </summary>
		private static string ModName = "DD Work Tab";

		/// <summary>
		/// Provides string caching (for translations).
		/// </summary>
		public static Dictionary<object, string> CachedStrings = new Dictionary<object, string>();

		/// <summary>
		/// Provides quick storage and access to translations. Prevents calling the .Translate() chain multiple times.
		/// </summary>
		public static string CachedTranslation(this string inputText, object[] args = null)
		{
			if (!CachedStrings.TryGetValue(inputText, out string finalString))
			{
				finalString = inputText.Translate();

				CachedStrings[inputText] = finalString;
			}

			if (args != null)
			{
				return string.Format(finalString, args);
			}

			return finalString;
		}

		/// <summary>
		/// Generates a high quality texture from a PNG file.
		/// </summary>
		public static Texture2D GetHQTexture(string fileName, string folderName = null)
		{
			Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false); //Mipmaps off;

			ModContentPack content = null;

			foreach (ModContentPack mod in LoadedModManager.RunningMods)
			{
				if (mod.Name == ModName)
				{
					content = mod;
				}
			}

			if (content == null)
			{
				Log.Error("Could not find specific mod :: " + ModName);

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
				Log.Message("Could not find specific file name :: " + fileName + ". Using empty texture instead.");

				Texture2D emptyTexture = ContentFinder<Texture2D>.Get("EmptyTexture");

				if (emptyTexture == null)
				{
					Log.Error("Could not default to empty texture. Please verify mod installation.");

					return texture;
				}

				return emptyTexture;
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
		public static void CacheScrollview(bool reverse, float scrolledY, float outerHeight, float itemHeight, int itemCount, ref float renderY, out int firstItem, out int lastItem)
		{
			int totalRenderedIndexes = (int)(outerHeight / itemHeight) + 2; //Account for partly rendered surfaces on top/bottom of the Rect

			firstItem = (int)(scrolledY / itemHeight); //Get the first list item that should be at least partly visible

			renderY += firstItem * itemHeight; //The .y value of the first rendered archive

			//Used when iterating forwards through a list (i++)
			if (!reverse)
			{
				lastItem = Mathf.Min(firstItem + totalRenderedIndexes, itemCount - 1); //Get the last item to render, don't go over list.Count
			}

			//Used when iterating backwards through a list (i--)
			else
			{
				firstItem = itemCount - 1 - firstItem;

				lastItem = Mathf.Max(firstItem - totalRenderedIndexes, -1); //Make sure to not >=LastIndex in the iteration
			}
		}
	}
}
