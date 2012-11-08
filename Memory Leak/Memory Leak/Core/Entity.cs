using System;
using System.Drawing;
using System.Linq;
using MemoryLeak.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
    public class Entity : Drawable
    {
        private Chunk.Tile _parentTile;

        public Chunk Parent { get; set; }
        public bool IsPassable { get; set; }
        public bool IsWalkable { get; set; }

        public new Vector2 Position
        {
            get { return new Vector2((float)Math.Round(base.Position.X), (float)Math.Round(base.Position.Y)); }
            set { base.Position = value; }
        }

        public Chunk.Tile ParentTile
        {
            get { return _parentTile; }
            set
            {
                Chunk.Tile old = null;
                if (ParentTile != null) old = ParentTile;
                _parentTile = value;

                if (old == null || (old == _parentTile))
                {
                    if (_parentTile != null && !_parentTile.Children.Contains(this)) _parentTile.Children.Add(this);
                    return;
                }

                if (old.Children.Contains(this)) old.Children.Remove(this);
                if (_parentTile != null && !_parentTile.Children.Contains(this)) ParentTile.Children.Add(this);
            }
        }

        public event Action<Drawable> Death, Collision;
        public event Action<float> Tick;

        public Entity(Texture2D texture, int x, int y, int z, bool isPassable = false)
            : base(texture)
        {
            Position = new Vector2(x * Width, y * Height);
            Depth = z;

            IsPassable = isPassable;
        }

        public void Kill(Entity killer)
        {
            if (Death != null) Death(killer);
            Parent.Remove(this);
        }

        public void Move(int x, int y, float rate)
        {
            rate = Math.Abs(rate);
            Position += new Vector2(x * rate, y * rate);

            if (Parent != null)
            {
                CorrectParent();
                Correct();
            }


            if (ParentTile != null) ParentTile.OnStep(this);
        }

        public void Correct()
        {
            Vector2 correction = Vector2.Zero;

            int xMax = (int)(Math.Round(Rectangle.Right) / Chunk.Tile.Width) + 1;
            int yMax = (int)(Math.Round(Rectangle.Bottom) / Chunk.Tile.Height) + 1;

            for (var x = (int)(Math.Round(Rectangle.X) / Chunk.Tile.Width) - 1; x < xMax; x++)
            {
                for (var y = (int)(Math.Round(Rectangle.Y) / Chunk.Tile.Height) - 1; y < yMax; y++)
                {
                    var tile = Parent != null
                                   ? Parent.Get(x, y, Depth)
                                   : null;

                    // Entity Pass
                    /*if(tile != null)
                    {
                        var duplicate = new Entity[tile.Children.Count];
                        tile.Children.CopyTo(duplicate);

                        foreach (var i in duplicate.Where(i => i != this && i.Rectangle.IntersectsWith(Rectangle)))
                        {
                            i.OnCollision(this);
                            OnCollision(i);
                        }

                        offset = duplicate.Where(i => i != this && i.Rectangle.IntersectsWith(Rectangle) && !i.IsPassable).Aggregate(offset, (current, i) => current + Offset(true, Rectangle, i.Rectangle));
                    }*/

                    // Tile & Faux-Tile Pass

                    RectangleF rectangle = tile == null ? new RectangleF(x * Chunk.Tile.Width, y * Chunk.Tile.Height, Width, Height) : tile.Rectangle;

                    if (!Rectangle.IntersectsWith(rectangle) || Rectangle == rectangle) continue;
                    if (tile != null && tile.IsPassable) continue;

                    var offset = Offset(Rectangle, rectangle);

                    if (Math.Abs(offset.X) > Math.Abs(correction.X)) correction.X = offset.X;
                    if (Math.Abs(offset.Y) > Math.Abs(correction.Y)) correction.Y = offset.Y;
                }
            }

            if(correction != Vector2.Zero) Position += correction;
        }

        private Vector2 Offset(RectangleF sender, RectangleF other)
        {
            var over = RectangleF.Intersect(sender, other);
            return new Vector2(sender.X < other.X ? over.Width : -over.Width, sender.Y < other.Y ? over.Height : -over.Height);
        }

        public void CorrectParent()
        {
            if (Parent == null) return;
            ParentTile = Parent.Get((int)(CenterPosition.X / Width), (int)(CenterPosition.Y / Height), Depth);
        }

        public void Update(float delta)
        {
            if (Tick != null) Tick(delta);
        }

        private void OnCollision(Drawable sender)
        {
            if (Collision != null) Collision(sender);
        }
    }
}
