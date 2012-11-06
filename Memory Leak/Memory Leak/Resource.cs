using System;
using System.Collections.Generic;
using System.IO;
using MemoryLeak.Audio;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak
{
	static class ResourceDirectory
	{
		public static string Base = "Content/";
		public static string Textures = "Textures/";
		public static string SoundEffects = "Audio/Sounds/";
	}

	static class Resource<T> where T : class
	{
		private static readonly Dictionary<string, T> Resources = new Dictionary<string, T>();

		public static T Get(string name)
		{
			switch (typeof(T).Name)
			{
				case "Texture2D":
					if (string.IsNullOrEmpty(ResourceDirectory.Textures)) throw new Exception("Texture Directory needs to be set");

					if (!Resources.ContainsKey(name))
					{
						var stream = new FileStream(ResourceDirectory.Base + ResourceDirectory.Textures + name + ".png", FileMode.Open);
						Resources.Add(name, Texture2D.FromStream(Game.Core.GraphicsDevice, stream) as T);
					}
					break;

				case "Sound":
					if (string.IsNullOrEmpty(ResourceDirectory.SoundEffects)) throw new Exception("Sound Effects Directory needs to be set");

					if (!Resources.ContainsKey(name))
					{
						var stream = new FileStream(ResourceDirectory.Base + ResourceDirectory.SoundEffects + name + ".wav", FileMode.Open);
						Resources.Add(name, Sound.FromStream(stream) as T);
					}
					break;

				default:
					throw new Exception("Missing resource type");
			}

			return Resources[name];
		}
	}
}
