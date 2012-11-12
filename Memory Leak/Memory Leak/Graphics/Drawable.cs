using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MemoryLeak.Graphics
{
    public class Drawable
    {
        public float Width { get { return Source.Width; } }
        public float Height { get { return Source.Height; } }

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }
        public float Depth { get; set; }
        public Color Color { get; set; }
        public Rectangle Source { get; set; }

        public RectangleF Rectangle
        {
            get { return new RectangleF(Position.X, Position.Y, Width * Scale.X, Height * Scale.Y); }
        }

        public Vector2 CenterPosition
        {
            get { return new Vector2(Position.X + ((Width * Scale.X) / 2), Position.Y + ((Height * Scale.Y) / 2)); }
        }

        public Drawable(Texture2D texture, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            Texture = texture;
            Position = Vector2.Zero;
            Origin = Vector2.Zero;
            Color = Color.White;
            Scale = Vector2.One;

            Source = new Rectangle(x, y, width ?? Texture.Width, height ?? Texture.Height);
        }

        public virtual void Draw(SpriteBatch spriteBatch, int maxDepth, byte darkness = 0)
        {
            if (Texture != null)
                spriteBatch.Draw(Texture, 
                    new Vector2(Rectangle.Left, Rectangle.Top), Source,
                    new Color(Color.R - darkness, Color.G - darkness, Color.B - darkness, Color.A),
                    Rotation,
                    Origin,
                    Scale,
                    SpriteEffects.None,
                    1 - (Depth / maxDepth));
        }
    }
}
