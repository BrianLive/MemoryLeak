using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Graphics
{
    public class Drawable
    {
        public const int Width = 32;
        public const int Height = 32;

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }

        public Rectangle Rectangle
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, Width, Height); }
        }

        public Vector2 CenterPosition
        {
            get { return new Vector2(Position.X + (Width / 2), Position.Y + (Height / 2)); }
        }

        public Drawable(Texture2D texture)
        {
            Texture = texture;
            Position = Vector2.Zero;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null) spriteBatch.Draw(Texture, Rectangle, Color.White);
        }
    }
}
