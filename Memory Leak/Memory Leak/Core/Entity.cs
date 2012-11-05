using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Core
{
    class Entity : Drawable
    {
        private Chunk.Tile _parentTile;

        public Chunk Parent { get; set; }
        public bool IsPassable { get; set; }

        public int EnergyCapacity { get; set; }
        public int EnergyPool { get; set; }
        public int EnergyPush { get; set; }

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

        public event Delegates.EntityHandler Death, Tick, Collision;

        public Entity(Texture2D texture, int x, int y, bool isPassable = false)
            : base(texture)
        {
            Position = new Vector2(x * Width, y * Height);
            IsPassable = isPassable;
            EnergyCapacity = 100;
            EnergyPool = EnergyCapacity;
            EnergyPush = 0;
        }

        public void Kill(Entity killer)
        {
            if (Death != null) Death(killer);
            Parent.Remove(this);
        }

        public void Move(int x, int y, float rate)
        {
            rate = Math.Abs(rate);
            Position += new Vector2(x * rate, 0);
            CorrectParent();
            Correct(0, 2);
            Position += new Vector2(0, y * rate);
            CorrectParent();
            Correct(1, 2);
            Correct(2, 2);

            if (ParentTile != null) ParentTile.OnStep(this);
        }

        public void Correct(int flag, int checkDistance = 1)
        {
            for (var xx = -checkDistance; xx <= checkDistance; xx++)
                for (var yy = -checkDistance; yy <= checkDistance; yy++)
                {
                    var tile = Parent.Get((int)Math.Round(CenterPosition.X / Width) + xx,
                                          (int)Math.Round(CenterPosition.Y / Height) + yy);

                    if (flag == 2)
                    {
                        if (tile == null) continue;

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
                        if (tile == null || !tile.Rectangle.Intersects(Rectangle)) continue;

                        if (!tile.IsPassable)
                        {
                            var over = Rectangle.Intersect(Rectangle, tile.Rectangle);

                            switch (flag)
                            {
                                case 0:
                                    if (CenterPosition.X < tile.CenterPosition.X) Move(-1, 0, over.Width);
                                    else Move(1, 0, over.Width);
                                    break;
                                case 1:
                                    if (CenterPosition.Y < tile.CenterPosition.Y) Move(0, -1, over.Height);
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
            ParentTile = Parent.Get((int)CenterPosition.X / Width, (int)CenterPosition.Y / Height);
        }

        public void Update(GameTime gameTime)
        {
            if (Tick != null) Tick(this);
            EnergyPool = EnergyCapacity;
            EnergyPush = 0;
        }

        private void OnCollision(Drawable sender)
        {
            if (Collision != null) Collision(sender);
        }
    }
}
