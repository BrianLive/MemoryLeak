using System;
using MemoryLeak.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
	// TODO: make this an abstract class - Rohan
    public class Entity : Drawable
    {
        public virtual Chunk Parent { get; set; }

        public event Action<Drawable> Death;
        public event Action<float> Tick;
        
        public Entity(Texture2D texture, int x, int y, int z)
            : base(texture)
        {
            Position = new Vector2(x * Width, y * Height);
            Depth = z;
        }

        public void Kill(Entity killer)
        {
            if (Death != null)
				Death(killer);

            Parent.Remove(this);
        }

        public void Update(float delta)
        {
            if (Tick != null)
				Tick(delta);
        }
    }
}
