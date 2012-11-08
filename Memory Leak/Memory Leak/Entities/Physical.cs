using System;
using MemoryLeak.Core;
using MemoryLeak.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MemoryLeak.Entities
{
    class Physical : Entity
    {
        public bool IsPassable { get; set; }
        public bool IsWalkable { get; set; }

        public event Action<Drawable> Collision;

        private float _hSave, _vSave;

        public Physical(Texture2D texture, int x, int y, int z, bool isPassable = false) : base(texture, x, y, z)
        {
            IsPassable = isPassable;
        }

        public void Move(int x, int y, float rate)
        {
            rate = Math.Abs(rate);

            float hMove = rate*x;
            float vMove = rate*y;

            int hRep = (int) Math.Floor(Math.Abs(hMove));
            int vRep = (int) Math.Floor(Math.Abs(vMove));

            _hSave += (float) (Math.Abs(hMove) - Math.Floor(Math.Abs(hMove)));
            _vSave += (float) (Math.Abs(vMove) - Math.Floor(Math.Abs(vMove)));

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
                testRect.X += Math.Sign(hMove);

                if(Parent.PlaceFree(testRect, Depth))
                {
                    _hSave = 0;
                    break;
                }

                Position += new Vector2(Math.Sign(hMove));
            }

            testRect = Rectangle;

            while(vRep-- > 0)
            {
                testRect.Y += Math.Sign(vMove);

                if(Parent.PlaceFree(testRect, Depth))
                {
                    _vSave = 0;
                    break;
                }

                Position += new Vector2(0, Math.Sign(vMove));
            }

            CorrectParent();
            if (ParentTile != null) ParentTile.OnStep(this);
        }

        private void OnCollision(Drawable sender)
        {
            if (Collision != null) Collision(sender);
        }
    }
}
