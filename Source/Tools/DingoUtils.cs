using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace DD_WorkTab
{
	public static class DingoUtils
	{
		/// <summary>
		/// The alphanumeric name of the mod folder, as defined in RimWorld/Mods.
		/// </summary>
		private const string ModFolderName = "DD WorkTab";

		/// <summary>
		/// The numeric folder name of the mod as a Workshop release, as defined in steamapps/workshop/content/294100.
		/// </summary>
		private const string WorkshopFolderName = "111111";

		/// <summary>
		/// Provides caching for various key types, depending on the mod's requirement.
		/// </summary>
		public static Dictionary<object, string> CachedStrings = new Dictionary<object, string>();

		/// <summary>
		/// Provides quick storage and access to translations. Circumvents calling the .Translate() chain more than once.
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

		/* Verse.ModContentLoader<T> */
		/// <summary>
		/// Generates a high quality texture from a PNG file.
		/// </summary>
		public static Texture2D GetHQTexture(string fileName, string folderName = null)
		{
			Texture2D texture = null;

			ModContentPack content = null;

			foreach (ModContentPack mod in LoadedModManager.RunningMods)
			{
				if (mod.Identifier == ModFolderName || mod.Identifier == WorkshopFolderName)
				{
					content = mod;
				}
			}

			if (content == null)
			{
				Log.Error("Could not find specific mod :: " + ModFolderName);

				return new Texture2D(1, 1);
			}

			string texturesPath = Path.Combine(content.RootDir, GenFilePaths.ContentPath<Texture2D>());

			string folderPath = folderName == null ? texturesPath : Path.Combine(new DirectoryInfo(texturesPath).ToString(), folderName);

			DirectoryInfo contentDirectory = new DirectoryInfo(folderPath);

			if (!contentDirectory.Exists)
			{
				Log.Error("Could not find specific textures folder");

				return new Texture2D(1, 1);
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

				return new Texture2D(1, 1);
			}

			byte[] fileData = File.ReadAllBytes(image.FullName);

			texture = new Texture2D(2, 2, TextureFormat.ARGB32, false); //Mipmaps off
			texture.LoadImage(fileData); //Loads image and resizes texture
			texture.name = "DD_" + Path.GetFileNameWithoutExtension(fileName);
			texture.filterMode = FilterMode.Trilinear;
			texture.anisoLevel = 9; //default 2, max 9

			/*	texture.Compress(true); //??? lowers quality
			 *	texture.Apply(false, true); //??? applies compression */

			if (texture == null)
			{
				Log.Error("Could not load high quality texture :: " + fileName);

				return new Texture2D(1, 1);
			}

			return texture;
		}
	}
}
