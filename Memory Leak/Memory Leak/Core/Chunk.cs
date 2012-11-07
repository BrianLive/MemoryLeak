using System;
using System.Collections.Generic;
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

            public Chunk Parent { get; set; }

            public List<Entity> Children = new List<Entity>();

            public event Action<Drawable> Step;

            public Tile(Texture2D texture)
                : base(texture)
            {
                IsPassable = false;
            }

            public void OnStep(Drawable sender)
            {
                var handler = Step;
                if (handler != null) handler(sender);
            }
        }

        private readonly Tile[,,] _tiles;
        private readonly List<Entity> _entities = new List<Entity>();
        private readonly List<DebugRectangle> _debuggers = new List<DebugRectangle>();

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
            _entities.Add(entity);
            entity.Parent = this;
            entity.CorrectParent();
            _debuggers.Add(entity.One);
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
            tile.Parent = this;
        }

        public Tile Get(int x, int y, int z)
        {
            if (x > Width - 1 || y > Height - 1 || x < 0 || y < 0) return null;
            return _tiles[x, y, z];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var xSize = (int)(Parent.Camera.Position.X - (Game.Core.Resolution.X / 2)) / (int)Tile.Width;
            var ySize = (int)(Parent.Camera.Position.Y - (Game.Core.Resolution.Y / 2)) / (int)Tile.Height;

            for (var x = xSize; x < xSize + (Game.Core.Resolution.X / 2); x++)
                for (var y = ySize; y < ySize + (Game.Core.Resolution.Y / 2); y++)
                    for (var z = 0; z < Depth; z++)
                        if (x < Width && y < Height && x >= 0 && y >= 0)
                            if (_tiles[x, y, z] != null)
                                _tiles[x, y, z].Draw(spriteBatch);

            foreach (var i in _entities)
                i.Draw(spriteBatch);

            foreach (var i in _debuggers)
                i.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var i in _entities)
                i.Update(gameTime);
        }
    }
}
