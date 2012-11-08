using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MemoryLeak.Utility
{
    public static class CollisionHelper
    {
        public struct CollisionVector
        {
            public int DirX, DirY;
            public float X, Y;
        }

        public static CollisionVector Offset(RectangleF owner, RectangleF sender)
        {
            var correction = new CollisionVector();

            float left = Math.Abs(owner.Right - sender.Left);
            float right = Math.Abs(owner.Left - sender.Right);
            float top = Math.Abs(owner.Bottom - sender.Top);
            float bottom = Math.Abs(owner.Top - sender.Bottom);

            if (left < right)
            {
                correction.X = left;
                correction.DirX = -1;
            }
            else if (left > right)
            {
                correction.X = right;
                correction.DirX = 1;
            }

            if (top < bottom)
            {
                correction.Y = top;
                correction.DirY = -1;
            }
            else if (top > bottom)
            {
                correction.X = bottom;
                correction.DirY = 1;
            }

            return correction;
        }

        public static CollisionVector GetSmallestCorrection(bool isHorizontal, int direction,
                                                            List<CollisionVector> corrections)
        {
            var smallest = new CollisionVector();

            if(isHorizontal)
            {
                
                smallest.X = int.MaxValue;

                foreach (var i in corrections.Where(i => i.DirX == direction && i.X < smallest.X))
                    smallest = i;

                return smallest;
            }
            else
            {
                smallest.Y = int.MaxValue;

                foreach (var i in corrections.Where(i => i.DirY == direction && i.Y < smallest.Y))
                    smallest = i;

                return smallest;
            }
        }

        public static void CorrectCollision(ref Vector2 position, CollisionVector correction, bool isHorizontal)
        {
            if (isHorizontal) position.X += correction.X*correction.DirX;
            else position.Y += correction.Y*correction.DirY;
        }
    }
}
