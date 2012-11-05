using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak
{
    class Resource<T> where T : class
    {
        private static readonly Dictionary<string, T> Resources = new Dictionary<string, T>();

        public static T Get(string name)
        {
            switch (typeof(T).Name)
            {
                case "Texture2D":
                    if (!Resources.ContainsKey(name))
                    {
                        var stream = new FileStream("Content/Textures/" + name + ".png", FileMode.Open);
                        Resources.Add(name, Texture2D.FromStream(Game.Graphics.GraphicsDevice, stream) as T);
                    }
                    break;
                case "SoundEffect":
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
