﻿using System;
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

        public event Action<Drawable> Death, Tick, Collision;

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
            Position += new Vector2(x*rate, 0);
            CorrectParent();
            CorrectNew(0);
            Position += new Vector2(0, y*rate);
            CorrectParent();
            CorrectNew(1);
            if (ParentTile != null) ParentTile.OnStep(this);
        }

        public void CorrectNew(int flag)
        {
            for(var x = -2; x <= 2; x++)
                for(var y = -2; y <= 2; y++)
                {
                    var tile = Parent.Get((int)Math.Round(CenterPosition.X / Width) + x,
                                          (int)Math.Round(CenterPosition.Y / Height) + y, Depth);

                    if (tile == null)
                    {
                        var tileLower = Depth == 0 ? null : Parent.Get((int)Math.Round((Position.X + (Width / 2)) / Width) + x,
                        (int)Math.Round((Position.Y + (Height / 32)) / Height) + y, Depth - 1);

                        if (tileLower == null || (!tileLower.Children.Any(i => i.IsWalkable) && !tileLower.IsRamp))
                        {
                            var rectangle = new Rectangle((int) (Math.Round(CenterPosition.X/Width) + x) * Width,
                                                          (int) (Math.Round(CenterPosition.Y/Height) + y) * Height, Width,
                                                          Height);

                            Console.WriteLine("player: " + Rectangle + "; null tile: " + rectangle);
                            if (Rectangle.Intersects(rectangle))
                                Offset(flag, rectangle);
                        }
                        else if(tileLower.IsRamp)
                        {
                            
                        }
                    }
                    else if (!tile.IsPassable && Rectangle.Intersects(tile.Rectangle))
                        Offset(flag, tile.Rectangle);
                }
        }

        private void Offset(int flag, Rectangle other)
        {
            var over = Rectangle.Intersect(Rectangle, other);

            if (over.Width != 0 && flag == 0)
            {
                if (CenterPosition.X < other.X) Move(-1, 0, over.Width);
                else Move(1, 0, over.Width);
            }

            if (over.Height != 0 && flag == 1)
            {
                if (CenterPosition.Y < other.Y) Move(0, -1, over.Height);
                else Move(0, 1, over.Height);
            }
        }

        public void Correct(int flag, int checkDistance = 2)
        {
            // Check Distance is the amount of tiles out that we want to check - Brian
            for (var xx = -checkDistance; xx <= checkDistance; xx++)
                for (var yy = -checkDistance; yy <= checkDistance; yy++)
                {
                    var tile = Parent.Get((int)Math.Round(CenterPosition.X / Width) + xx,
                                          (int)Math.Round(CenterPosition.Y / Height) + yy, Depth);

                    if (flag == 2) // Entity Collision Pass - Brian
                    {
                        if (tile == null) continue; // If this tile doesn't exist, move to the next tile. - Brian

                        /* We cannot modify an active list
                         * So we duplicate it and make our changes to the duplicate
                         * Later on in the code we apply these changes to the old list, while it is inactive. 
                         * - Brian*/
                        var duplicate = new Entity[tile.Children.Count];
                        tile.Children.CopyTo(duplicate);

                        foreach (var i in duplicate.Where(i => i != this && i.Rectangle.Intersects(Rectangle)))
                        {
                            i.OnCollision(this);
                            OnCollision(i);
                        }

                        foreach (var i in duplicate.Where(i => i != this && i.Rectangle.Intersects(Rectangle) && !i.IsPassable))
                        {
                            var overEnt = Rectangle.Intersect(Rectangle, i.Rectangle);
                            var entityDirection = (overEnt.Width < overEnt.Height);

                            Entity pusher = this, pushee = i;

                            if (entityDirection)
                            {
                                if (pusher.CenterPosition.X < pushee.CenterPosition.X) pushee.Move(1, 0, overEnt.Width);
                                else pushee.Move(-1, 0, overEnt.Width);
                            }
                            else
                            {
                                if (pusher.Position.Y < pushee.CenterPosition.Y) pushee.Move(0, 1, overEnt.Height);
                                else pushee.Move(0, -1, overEnt.Height);
                            }
                        }
                    }
                    else
                    {
                        if (tile == null || !tile.IsPassable)
                        {
                            var rectangle = tile == null
                                                ? new Rectangle((int) (Math.Round(CenterPosition.X/Width) + xx)*Width,
                                                                (int) (Math.Round(CenterPosition.Y/Height) + yy)*Height,
                                                                Width,
                                                                Height)
                                                : tile.Rectangle;

                            var over = Rectangle.Intersect(Rectangle, rectangle);

                            switch (flag)
                            {
                                case 0:
                                    if (CenterPosition.X < rectangle.X) Move(-1, 0, over.Width);
                                    else Move(1, 0, over.Width);
                                    break;
                                case 1:
                                    if (CenterPosition.Y < rectangle.Y) Move(0, -1, over.Height);
                                    else Move(0, 1, over.Height);
                                    break;
                            }
                        }
                        else tile.OnStep(this);
                    }
                }
        }

        public void CorrectParent()
        {
            if (Parent == null) return;
            ParentTile = Parent.Get((int)CenterPosition.X / Width, (int)CenterPosition.Y / Height, Depth);
        }

        public void Update(GameTime gameTime)
        {
            if (Tick != null) Tick(this);
        }

        private void OnCollision(Drawable sender)
        {
            if (Collision != null) Collision(sender);
        }
    }
}
