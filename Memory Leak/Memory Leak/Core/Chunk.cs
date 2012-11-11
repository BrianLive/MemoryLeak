using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MemoryLeak.Entities;
using MemoryLeak.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
    public class Chunk
    {
        public class Tile : Drawable
        {
            public new const float Width = 32;
            public new const float Height = 32;

            public bool IsPassable { get; set; }
            public bool IsRamp { get; set; }
            public bool IsRampHorizontal { get; set; }
            public bool IsRampUpNegative { get; set; }
            public bool IsFloater { get; set; }
            public bool IsFloaterLayered { get; set; }
            
            public Chunk Parent { get; set; }

            public List<Entity> Children = new List<Entity>();

            public event Action<Physical> Step;

            public Tile(Texture2D texture, int x = 0, int y = 0, int width = -1, int height = -1)
                : base(texture, x, y, width, height)
            {
                IsPassable = false;
                IsRamp = false;
                IsRampHorizontal = false;
                IsRampUpNegative = false;
                IsFloater = false;
                IsFloaterLayered = false;
            }

            public void OnStep(Physical sender)
            {
                var startDepth = sender.Depth;

                if (IsRamp)
                {
                    Console.WriteLine("called it");
                    if (IsRampHorizontal)
                    {
                        if(sender.DirX == 1)
                        {
                            if (IsRampUpNegative)
                            {
                                if (sender.Rectangle.Left >= Rectangle.Left) sender.Depth = Depth;
                            }
                            else
                            {
                                if (sender.Rectangle.Left >= Rectangle.Left) sender.Depth = Depth + 1;
                            }
                        }
                        else if(sender.DirX == -1)
                        {
                            if (IsRampUpNegative)
                            {
                                if (sender.Rectangle.Right <= Rectangle.Right) sender.Depth = Depth + 1;
                            }
                            else
                            {
                                if (sender.Rectangle.Right <= Rectangle.Right) sender.Depth = Depth;
                            }
                        }
                    }
                    else
                    {
                        if (sender.DirY == 1)
                        {
                            if (IsRampUpNegative)
                            {
                                if (sender.Rectangle.Top >= Rectangle.Top) sender.Depth = Depth;
                            }
                            else
                            {
                                if (sender.Rectangle.Top >= Rectangle.Top) sender.Depth = Depth + 1;
                            }
                        }
                        else if (sender.DirY == -1)
                        {
                            if (IsRampUpNegative)
                            {
                                if (sender.Rectangle.Bottom <= Rectangle.Bottom) sender.Depth = Depth + 1;
                            }
                            else
                            {
                                if (sender.Rectangle.Bottom <= Rectangle.Bottom) sender.Depth = Depth;
                            }
                        }
                    }

                    foreach(var rectangle in Parent.GetNearbyRects(sender.Rectangle, sender.Depth))
                    {
                        int x = (int) Math.Round(rectangle.X/Width);
                        int y = (int) Math.Round(rectangle.Y/Height);
                        int z = (int) Math.Round(sender.Depth);

                        var tile = Parent.Get(x, y, z);

                        if(sender.Rectangle.IntersectsWith(rectangle))
                        {
                            var lower = Parent.Get(x, y, z - 1);

                            if ((tile != null && (tile.IsRamp || tile.IsPassable)) || (lower != null && lower.IsRamp)) continue;

                            sender.Depth = startDepth;
                            break;
                        }
                    }

                    sender.Depth = (int)MathHelper.Clamp(sender.Depth, 0, Parent.Depth);
                }

                var handler = Step;
                if (handler != null) handler(sender);
            }
        }

        private readonly Tile[,,] _tiles;
        private readonly List<Entity> _entities = new List<Entity>();

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int Depth { get; private set; }

        public State Parent { get; set; }

        public Chunk(int width, int height, int depth = 1)
        {
            Width = width;
            Height = height;
            Depth = depth;

            _tiles = new Tile[width, height, depth];

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    _tiles[x, y, 0] = null;
        }

        public void Add(Entity entity)
        {
            entity.Parent = this;
            _entities.Add(entity);

            if(entity as Physical != null)
                ((Physical)entity).CorrectParent();
        }

        public void Remove(Entity entity)
        {
            _entities.Remove(entity);
        }

        public void Set(int x, int y, int z, Tile tile)
        {
            x = Math.Max(0, Math.Min(x, Width - 1));
            y = Math.Max(0, Math.Min(y, Height - 1));
            z = Math.Max(0, Math.Min(z, Depth - 1));

            _tiles[x, y, z] = tile;

            if (tile == null) return;
            tile.Position = new Vector2(x * Tile.Width, y * Tile.Height);
            tile.Depth = z;
            tile.Parent = this;
        }

        public Tile Get(int x, int y, int z)
        {
            if (x < Width && y < Height && z < Depth && x >= 0 && y >= 0 && z >= 0) return _tiles[x, y, z];
            return null;
        }

        public List<RectangleF> GetNearbyRects(RectangleF rect, float depth)
        {
            var tiles = new List<RectangleF>();
            int z = (int) (float) Math.Floor(depth);
            int xMax = (int) (Math.Round(rect.Right)/Tile.Width) + 1;
            int yMax = (int) (Math.Round(rect.Bottom)/Tile.Height) + 1;

            for (var x = (int) (Math.Round(rect.X)/Tile.Width) - 1; x < xMax; x++)
            {
                for (var y = (int) (Math.Round(rect.Y)/Tile.Height) - 1; y < yMax; y++)
                {
                    var tile = Get(x, y, z);

                    RectangleF rectangle = tile == null
                                               ? new RectangleF(x*Tile.Width, y*Tile.Height, Tile.Width, Tile.Height)
                                               : tile.Rectangle;

                    tiles.Add(rectangle);
                }
            }

            return tiles;
        }

        public bool PlaceFree(Physical sender, RectangleF rect, float depth)
        {
            foreach(var rectangle in GetNearbyRects(rect, depth))
            {
                int x = (int)Math.Round(rectangle.X / Tile.Width);
                int y = (int)Math.Round(rectangle.Y / Tile.Height);
                int z = (int)Math.Round(depth);

                var tile = Get(x, y, z);
                var lower = Get(x, y, z - 1);

                if (rect.IntersectsWith(rectangle) && (lower != null && lower.IsRamp && tile == null))
                {
                    lower.OnStep(sender);
                    continue;
                }

                if (rect.IntersectsWith(rectangle) && ((tile != null && !tile.IsPassable) || tile == null))
                    return false;

                if (tile != null)
                {
                    foreach (var ii in tile.Children.OfType<Physical>().Where(ii => ii != sender).Where(ii => !ii.IsPassable && rect.IntersectsWith(ii.Rectangle)))
                    {
                        sender.OnCollision(ii);
                        ii.OnCollision(sender);
                        return false;
                    }
                }
                else if (lower != null)
                {
                    if (lower.Children.OfType<Physical>().Any(ii => !ii.IsWalkable))
                        return false;
                }
            }

            return true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            int playerDepth = (int) Parent.Player.Depth + 1;
            
            // Draw tiles here
            foreach(var tile in _tiles)
            {
                if (tile == null) continue;
                if (tile.Depth > playerDepth) continue;
                if (tile.IsFloater && !tile.IsFloaterLayered && Math.Abs(tile.Depth - Parent.Player.Depth) < float.Epsilon) continue;

                if (tile.IsFloater && tile.Depth > Parent.Player.Depth) tile.Draw(spriteBatch, Depth);
                else tile.Draw(spriteBatch, Depth, (byte)((Parent.Player.Depth - tile.Depth) * (255f / Depth)));
            }

            //Draw entities here
            foreach (var i in _entities)
            {
                if (i.Depth > playerDepth) continue; // Don't draw entities above this layer
                i.Depth += 0.5f;
                i.Draw(spriteBatch, Depth, (byte)((Parent.Player.Depth - i.Depth) * (255f / Depth)));
                i.Depth -= 0.5f;
            }
        }

        public void Update(float delta)
        {
            foreach (var i in _entities)
                i.Update(delta);
        }
    }
}
