using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace MemoryLeak.Graphics
{
    public class Drawable
    {
        public float Width = 32;
        public float Height = 32;

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }
        public int Depth { get; set; }
        public Color Color { get; set; }

        public RectangleF Rectangle
        {
            get { return new RectangleF(Position.X, Position.Y, Width * Scale.X, Height * Scale.Y); }
        }

        public Vector2 CenterPosition
        {
            get { return new Vector2(Position.X + ((Width * Scale.X) / 2), Position.Y + ((Height * Scale.Y) / 2)); }
        }

        public Drawable(Texture2D texture)
        {
            Texture = texture;
            Position = Vector2.Zero;
            Origin = Vector2.Zero;
            Color = Color.White;
            Scale = Vector2.One;
            Width = Texture.Width;
            Height = Texture.Height;
        }

        public virtual void Draw(SpriteBatch spriteBatch, int maxDepth, byte darkness = 0)
        {
            if (Texture != null)
                spriteBatch.Draw(Texture, new Vector2(Rectangle.Left, Rectangle.Top), null, new Color(Color.R - darkness, Color.G - darkness, Color.B - darkness, Color.A), Rotation, Origin, Scale, SpriteEffects.None, Depth / maxDepth);
        }
    }
}
