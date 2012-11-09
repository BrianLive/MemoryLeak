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
            
            public Chunk Parent { get; set; }

            public List<Entity> Children = new List<Entity>();

            public event Action<Physical> Step;

            public Tile(Texture2D texture)
                : base(texture)
            {
                IsPassable = false;
                IsRamp = false;
                IsRampHorizontal = false;
                IsRampUpNegative = false;
            }

            public void OnStep(Physical sender)
            {
                if (IsRamp)
                {
                    if (IsRampHorizontal)
                    {
                        if(sender.DirX == 1)
                        {
                                var depthOffset = (sender.Rectangle.Left > Rectangle.Left) ? 1 : -1;
                                sender.Depth = (Depth + (IsRampUpNegative ? -1 : 1)) * depthOffset;
                        }
                        else if(sender.DirX == -1)
                        {
                            var depthOffset = (sender.Rectangle.Right > Rectangle.Right) ? 1 : -1;
                            sender.Depth = Depth + (IsRampUpNegative ? -1 : 1) * depthOffset;
                        }

                        sender.Depth = (int)MathHelper.Clamp(sender.Depth, 0, Parent.Depth - 1);
                    }
                    else
                    {
                    }

                    Console.WriteLine(sender.Depth);
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
            x = Math.Max(0, x);
            y = Math.Max(0, y);
            z = Math.Max(0, z);

            _tiles[x, y, z] = tile;
            tile.Position = new Vector2(x * Tile.Width, y * Tile.Height);
            tile.Depth = z;
            tile.Parent = this;
        }

        public Tile Get(int x, int y, int z)
        {
            if (x > Width - 1 || y > Height - 1 || z > Depth - 1 || x < 0 || y < 0 || z < 0) return null;
            return _tiles[x, y, z];
        }

        public bool PlaceFree(Physical sender, RectangleF rect, int z)
        {
            int xMax = (int) (Math.Round(rect.Right)/Tile.Width) + 1;
            int yMax = (int) (Math.Round(rect.Bottom)/Tile.Height) + 1;

            for (var x = (int) (Math.Round(rect.X)/Tile.Width) - 1; x < xMax; x++)
            {
                for (var y = (int) (Math.Round(rect.Y)/Tile.Height) - 1; y < yMax; y++)
                {
                    var tile = Get(x, y, z);
                    var lower = Get(x, y, z - 1);

                    RectangleF rectangle = tile == null
                                               ? new RectangleF(x*Tile.Width, y*Tile.Height, Tile.Width, Tile.Height)
                                               : tile.Rectangle;

                    if (rect.IntersectsWith(rectangle) && (lower != null && lower.IsRamp && tile == null))
                    {
                        lower.OnStep(sender);
                        return true;
                    }

                    if(rect.IntersectsWith(rectangle) && ((tile != null && !tile.IsPassable) || tile == null))
                    return false;

                    if (tile != null)
                    {
                        foreach (var i in tile.Children)
                        {
                            var ii = i as Physical;
                            if (ii == null) continue;
                            if (ii == sender) continue;
                            if (ii.IsPassable || !rect.IntersectsWith(ii.Rectangle)) continue;
                            sender.OnCollision(ii);
                            ii.OnCollision(sender);
                            return false;
                        }
                    }
                    else if(lower != null)
                    {
                        if (lower.Children.OfType<Physical>().Any(ii => !ii.IsWalkable))
                            return false;
                    }
                }
            }

            return true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var xSize = (int)(((Parent.Camera.Position.X - (Game.Core.Resolution.X / 2)) / (int)Tile.Width) * Parent.Camera.Zoom);
            var ySize = (int)(((Parent.Camera.Position.Y - (Game.Core.Resolution.Y / 2)) / (int)Tile.Height) * Parent.Camera.Zoom);

            for (var x = xSize; x < xSize + (Game.Core.Resolution.X / 2); x++)
                for (var y = ySize; y < ySize + (Game.Core.Resolution.Y / 2); y++)
                    //for (var z = 0; z < Depth; z++)
                        if (x < Width && y < Height && x >= 0 && y >= 0)
                            if (_tiles[x, y, Parent.Player.Depth] != null)
                                _tiles[x, y, Parent.Player.Depth].Draw(spriteBatch);

            foreach (var i in _entities)
                i.Draw(spriteBatch);
        }

        public void Update(float delta)
        {
            foreach (var i in _entities)
                i.Update(delta);
        }
    }
}
