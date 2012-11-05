using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak
{
    public class ResourceDirectory
    {
        public static String Textures { get; set; }
        public static String SoundEffects { get; set; }
    }

    class Resource<T> where T : class
    {
        private static readonly Dictionary<string, T> Resources = new Dictionary<string, T>();

        public static T Get(string name)
        {
            switch (typeof(T).Name)
            {
                case "Texture2D":
                    if(ResourceDirectory.Textures == null) throw new Exception("Texture Directory needs to be set.");

                    if (!Resources.ContainsKey(name))
                    {
                        var stream = new FileStream("Content/Textures/" + name + ".png", FileMode.Open);
                        Resources.Add(name, Texture2D.FromStream(Game.Core.GraphicsDevice, stream) as T);
                    }
                    break;
                case "SoundEffect":
                    if (ResourceDirectory.SoundEffects == null) throw new Exception("Sound Effects Directory needs to be set.");

                    if (!Resources.ContainsKey(name))
                    {
                        var stream = new FileStream("Content/Audio/Sounds/" + name + ".wav", FileMode.Open);
                        Resources.Add(name, SoundEffect.FromStream(stream) as T);
                    }
                    break;
            }

            return Resources[name];
        }
    }
}
