using System;
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

        private Rectangle _previousCollisionRect;

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

            Position += new Vector2(x*rate, y*rate);
            CorrectParent();
            Correct();

            if (ParentTile != null) ParentTile.OnStep(this);
        }

        public void Correct()
        {
            for(var x = -2; x <= 2; x++)
                for(var y = -2; y <= 2; y++)
                {
                    var tile = Parent.Get((int)Math.Round(CenterPosition.X / Width) + x,
                                          (int)Math.Round(CenterPosition.Y / Height) + y, Depth);

                    // Entity Pass
                    if(tile != null)
                    {
                        var duplicate = new Entity[tile.Children.Count];
                        tile.Children.CopyTo(duplicate);

                        foreach (var i in duplicate.Where(i => i != this && i.Rectangle.Intersects(Rectangle)))
                        {
                            i.OnCollision(this);
                            OnCollision(i);
                        }

                        foreach (var i in duplicate.Where(i => i != this && i.Rectangle.Intersects(Rectangle) && !i.IsPassable))
                            Offset(i.Rectangle);
                    }

                    // Tile & Faux-Tile Pass
                    Rectangle rectangle;

                    if (tile == null)
                        rectangle = new Rectangle((int)((Math.Round(CenterPosition.X / Width) + x) * Width),
                                                  (int)((Math.Round(CenterPosition.Y / Height) + y) * Height),
                                                  (int)Width,
                                                  (int)Height);
                    else rectangle = tile.Rectangle;

                    if (!rectangle.Intersects(Rectangle)) continue;

                    if (tile == null || !tile.IsPassable)
                        Offset(rectangle);
                }
        }

        private void Offset(Rectangle other)
        {
            var over = Rectangle.Intersect(Rectangle, other);

            if(over.Width < over.Height) OffsetDirection(true, other, over);
            else if(over.Width > over.Height) OffsetDirection(false, other, over);
            else if(over.Width == over.Height) 
                if(!_previousCollisionRect.IsEmpty)
                    if(_previousCollisionRect.Width < _previousCollisionRect.Height)
                        OffsetDirection(true, other, over);
                    else if (_previousCollisionRect.Width > _previousCollisionRect.Height)
                        OffsetDirection(false, other, over);
            
            _previousCollisionRect = over;
        }

        private void OffsetDirection(bool isHorizontal, Rectangle other, Rectangle over)
        {
            if (isHorizontal)
            {
                var isNegative = CenterPosition.X < (other.X + (other.Width / 2));

                if (isNegative) Move(-1, 0, over.Width);
                else Move(1, 0, over.Width);

                if (Rectangle.Intersects(other)) Offset(other);
            }
            else
            {
                var isNegative = CenterPosition.Y < (other.Y + (other.Height/2));

                if (isNegative) Move(0, -1, over.Height);
                else Move(0, 1, over.Height);

                if (Rectangle.Intersects(other)) Offset(other);
            }
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
