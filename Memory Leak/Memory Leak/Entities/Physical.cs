using System;
using System.Collections.Generic;
using System.Linq;
using MemoryLeak.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Entities
{
    public class Physical : Entity
    {
        public bool IsPassable { get; set; }
        public bool IsWalkable { get; set; }

        public int DirX { get; private set; }
        public int DirY { get; private set; }

        private Vector2 _velocity = Vector2.Zero;
        public Vector2 Velocity { get { return _velocity; } }

        public event Action<Physical> Collision;
        private readonly List<Chunk.Tile> _parentTiles = new List<Chunk.Tile>();

        private float _hSave, _vSave;

        public Chunk.Tile ParentTile
        {
            set
            {
                var tiles = new Chunk.Tile[_parentTiles.Count];
                _parentTiles.CopyTo(tiles);

                foreach (var i in tiles.Where(i => !Rectangle.IntersectsWith(i.Rectangle)))
                {
                    _parentTiles.Remove(i);
                }

                _parentTiles.Add(value);
            }
        }

        public Physical(Texture2D texture, int x, int y, int z, bool isPassable = false) : base(texture, x, y, z)
        {
            IsPassable = isPassable;
        }

        public void Move(int x, int y, float rate)
        {
            rate = Math.Abs(rate);

            _velocity = new Vector2(rate*x, rate*y);

            DirX = _velocity.X > 0 ? 1 : -1;
            DirY = _velocity.Y > 0 ? 1 : -1;

            int hRep = (int) Math.Floor(Math.Abs(_velocity.X));
            int vRep = (int) Math.Floor(Math.Abs(_velocity.Y));

            _hSave += (float) (Math.Abs(_velocity.X) - Math.Floor(Math.Abs(_velocity.X)));
            _vSave += (float) (Math.Abs(_velocity.Y) - Math.Floor(Math.Abs(_velocity.Y)));

            while(_hSave >= 1.0)
            {
                --_hSave;
                ++hRep;
            }

            while(_vSave >= 1.0)
            {
                --_vSave;
                ++vRep;
            }

            var testRect = Rectangle;

            while(hRep-- > 0)
            {
                testRect.X += Math.Sign(_velocity.X);

                if(!Parent.PlaceFree(this, testRect, Depth))
                {
                    _hSave = 0;
                    break;
                }

                Position += new Vector2(Math.Sign(_velocity.X), 0);
                CorrectParent();
            }

            testRect = Rectangle;

            while(vRep-- > 0)
            {
                testRect.Y += Math.Sign(_velocity.Y);

                if(!Parent.PlaceFree(this, testRect, Depth))
                {
                    _vSave = 0;
                    break;
                }

                Position += new Vector2(0, Math.Sign(_velocity.Y));
                CorrectParent();
            }

            foreach(var i in _parentTiles)
                i.OnStep(this);
        }

        public void CorrectParent()
        {
            if (Parent == null) return;

            int xMax = (int) (Math.Round(Rectangle.Right)/Chunk.Tile.Width) + 1;
            int yMax = (int) (Math.Round(Rectangle.Bottom)/Chunk.Tile.Height) + 1;

            for (var x = (int)(Math.Round(Rectangle.X) / Chunk.Tile.Width) - 1; x < xMax; x++)
            {
                for (var y = (int) (Math.Round(Rectangle.Y)/Chunk.Tile.Height) - 1; y < yMax; y++)
                {
                    var tile = Parent.Get(x, y, Depth);
                    if (tile != null && Rectangle.IntersectsWith(tile.Rectangle))
                        ParentTile = tile;
                }
            }
        }

        public void OnCollision(Physical sender)
        {
            if (Collision != null) Collision(sender);
        }
    }
}
