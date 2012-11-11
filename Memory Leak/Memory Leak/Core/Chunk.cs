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
            /// <summary>
            /// The common width of tiles.
            /// </summary>
            public new const float Width = 32;

            /// <summary>
            /// The common height of tiles.
            /// </summary>
            public new const float Height = 32;

            /// <summary>
            /// Decides if entities can pass through this tile.
            /// </summary>
            public bool IsPassable { get; set; }

            /// <summary>
            /// Decides if entities can use this tile as a ramp.
            /// </summary>
            public bool IsRamp { get; set; }

            /// <summary>
            /// Decides the direction of the ramp.
            /// </summary>
            public bool IsRampHorizontal { get; set; }

            /// <summary>
            /// Decides which direction of the ramp goes up and down.
            /// </summary>
            public bool IsRampUpNegative { get; set; }

            /// <summary>
            /// Decides if the tile is visible when the player is below it.
            /// </summary>
            public bool IsFloater { get; set; }

            /// <summary>
            /// Decides if the block should disappear or not when the player is on the layer the block is.
            /// </summary>
            public bool IsFloaterLayered { get; set; }

            /// <summary>
            /// Decides how much slower/faster the entity on this block should move.
            /// </summary>
            public float FrictionMultiplier { get; set; }

            /// <summary>
            /// The Chunk that this tile belongs to.
            /// </summary>
            public Chunk Parent { get; set; }

            /// <summary>
            /// A list of entities that are currently touching this tile.
            /// </summary>
            public List<Entity> Children = new List<Entity>();

            /// <summary>
            /// The event that is called when an entity touches this tile.
            /// </summary>
            public event Action<Physical> Step;

            /// <summary>
            /// Tile's are the foundation of any level.
            /// </summary>
            /// <param name="texture">The texture atlas that the tile will use.</param>
            /// <param name="x">The x position of the source rectangle.</param>
            /// <param name="y">The y position of the source rectangle</param>
            /// <param name="width">The width of the source rectangle.</param>
            /// <param name="height">The height of the source rectangle.</param>
            public Tile(Texture2D texture, int x = 0, int y = 0, int width = -1, int height = -1)
                : base(texture, x, y, width, height)
            {
                IsPassable = false;
                IsRamp = false;
                IsRampHorizontal = false;
                IsRampUpNegative = false;
                IsFloater = false;
                IsFloaterLayered = false;
                FrictionMultiplier = 1;
            }

            /// <summary>
            /// Called when an entity touches a tile.
            /// This method is required for ramps to work.
            /// </summary>
            /// <param name="sender">The entity that touched the tile.</param>
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

        /// <summary>
        /// Bi-Dimensional Array of tiles.
        /// </summary>
        private readonly Tile[,,] _tiles;

        /// <summary>
        /// List of entities contained in the chunk.
        /// </summary>
        private readonly List<Entity> _entities = new List<Entity>();

        /// <summary>
        /// The width of the chunk (in tiles).
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the chunk (in tiles).
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The amount of layers in the chunk.
        /// </summary>
        public int Depth { get; private set; }

        /// <summary>
        /// The state that this chunk belongs to.
        /// </summary>
        public State Parent { get; set; }

        /// <summary>
        /// Chunks are regions of land that the player explores.
        /// </summary>
        /// <param name="width">The width of the map (in tiles).</param>
        /// <param name="height">The height of the map (in tiles).</param>
        /// <param name="depth">The amount of layers in the chunk.</param>
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

        /// <summary>
        /// Adds an entity to the chunk.
        /// </summary>
        /// <param name="entity">The entity to be added.</param>
        public void Add(Entity entity)
        {
            entity.Parent = this;
            _entities.Add(entity);

            if(entity as Physical != null)
                ((Physical)entity).CorrectParent();
        }

        /// <summary>
        /// Removes an entity from the chunk.
        /// </summary>
        /// <param name="entity">The entity to be removed.</param>
        public void Remove(Entity entity)
        {
            _entities.Remove(entity);
        }

        /// <summary>
        /// Sets a tile to another tile.
        /// </summary>
        /// <param name="x">The x location on the chunk (in tiles).</param>
        /// <param name="y">The y location on the chunk (in tiles).</param>
        /// <param name="z">The layer on the chunk.</param>
        /// <param name="tile">The tile to be swapped in.</param>
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

        /// <summary>
        /// Retrieves a tile from the tile list.
        /// </summary>
        /// <param name="x">The x location on the chunk (in tiles).</param>
        /// <param name="y">The y location on the chunk (in tiles).</param>
        /// <param name="z">The layer on the chunk.</param>
        /// <returns>A tile.</returns>
        public Tile Get(int x, int y, int z)
        {
            if (x < Width && y < Height && z < Depth && x >= 0 && y >= 0 && z >= 0) return _tiles[x, y, z];
            return null;
        }

        /// <summary>
        /// Retrieves rectangles near a specified rectangle.
        /// </summary>
        /// <param name="rect">The base rectangle to expand and check around.</param>
        /// <param name="depth">The layer that is being checked.</param>
        /// <returns>The list of nearby tile rectangles.</returns>
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

        /// <summary>
        /// Checks whether or not a rectangle is colliding with impassible objects.
        /// </summary>
        /// <param name="sender">The object being checked against.</param>
        /// <param name="rect">The temporary rectangle from that object.</param>
        /// <param name="depth">The layer that is being checked.</param>
        /// <returns>Whether or not the requested location is free.</returns>
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

        /// <summary>
        /// Draws all entities and tiles belonging to the chunk.
        /// </summary>
        /// <param name="spriteBatch">The spritebatch to add to.</param>
        public void Draw(SpriteBatch spriteBatch)
        {
            int playerDepth = (int) Parent.Player.Depth;
            
            // Draw tiles here
            foreach(var tile in _tiles)
            {
                if (tile == null) continue;
                if (tile.Depth > playerDepth && !tile.IsFloater) continue;
                if (tile.IsFloater && !tile.IsFloaterLayered && Math.Abs(tile.Depth - (Parent.Player.Depth + 1)) < float.Epsilon) continue;

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

        /// <summary>
        /// Updates all the entities belonging to the chunk.
        /// </summary>
        /// <param name="delta">The difference in time between this frame and the last.</param>
        public void Update(float delta)
        {
            foreach (var i in _entities)
                i.Update(delta);
        }
    }
}
