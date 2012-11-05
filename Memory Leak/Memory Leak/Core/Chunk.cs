using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
    class Chunk
    {
        public class Tile : Drawable
        {
            public bool IsPassable { get; set; }
            public Chunk Parent { get; set; }

            public List<Entity> Children = new List<Entity>();

            public event Delegates.EntityHandler Step;

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

        private readonly Tile[,] _tiles;
        private readonly List<Entity> _entities = new List<Entity>();

        public int Width { get; set; }
        public int Height { get; set; }

        public State Parent { get; set; }

        public Chunk(int width, int height)
        {
            Width = width;
            Height = height;

            _tiles = new Tile[width, height];

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    _tiles[x, y] = new Tile(Resource<Texture2D>.Get("debug")) { Position = new Vector2(x * Drawable.Width, y * Drawable.Height), IsPassable = true };
        }

        public void Add(Entity entity)
        {
            _entities.Add(entity);
            entity.Parent = this;
            entity.CorrectParent();
        }

        public void Remove(Entity entity)
        {
            _entities.Remove(entity);
        }

        public void Set(int x, int y, Tile tile)
        {
            x = Math.Max(0, x);
            y = Math.Max(0, y);

            _tiles[x, y] = tile;
            tile.Position = new Vector2(x * Drawable.Width, y * Drawable.Height);
            tile.Parent = this;
        }

        public Tile Get(int x, int y)
        {
            if (x > Width - 1 || y > Height - 1 || x < 0 || y < 0) return null;
            return _tiles[x, y];
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var xSize = (int)(Parent.Camera.Position.X - (Game.Core.Resolution.X / 2)) / Drawable.Width;
            var ySize = (int)(Parent.Camera.Position.Y - (Game.Core.Resolution.Y / 2)) / Drawable.Height;

            for (var x = xSize; x < xSize + (Game.Core.Resolution.X / 2); x++)
                for (var y = ySize; y < ySize + (Game.Core.Resolution.Y / 2); y++)
                    if (x < Width && y < Height && x >= 0 && y >= 0)
                        if (_tiles[x, y] != null)
                            _tiles[x, y].Draw(spriteBatch);

            foreach (var i in _entities)
                i.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            foreach (var i in _entities)
                i.Update(gameTime);
        }
    }
}
