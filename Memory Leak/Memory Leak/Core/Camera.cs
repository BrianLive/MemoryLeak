using System;
using Microsoft.Xna.Framework;

namespace MemoryLeak.Core
{
    public class Camera
    {
        private Matrix _matrix;
        private Vector2 _position = Vector2.Zero;
        private Vector2 _actualPosition = Vector2.Zero;
        private float _rotation;
        private float _zoom = 1;
        private bool _isDirty = true;

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                _isDirty = true;
            }
        }

        public Vector2 ActualPosition
        {
            get { return _actualPosition; }
            private set
            {
                _actualPosition = value;
                _isDirty = true;
            }
        }

        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _isDirty = true;
            }
        }

        public float Zoom
        {
            get { return _zoom; }
            set
            {
                _zoom = (_zoom < 0.1f ? 0.1f : value);
                _isDirty = true;
            }
        }

        public Matrix Matrix
        {
            get
            {
                if(ActualPosition != _position)
                {
                    var x = 0f;
                    var y = 0f;

                    var xDelta = ActualPosition.X - _position.X;
                    var yDelta = ActualPosition.Y - _position.Y;

                    if(Math.Abs(xDelta) > float.Epsilon)
                    {
                        if (Math.Abs(xDelta) < 1) x = xDelta;
                        else x = xDelta * (ActualPosition.X / _position.X);

                    }
                    else if(Math.Abs(yDelta) > float.Epsilon)
                    {
                        if (Math.Abs(yDelta) < 1) y = yDelta;
                        else y = yDelta*(ActualPosition.Y/_position.Y);
                    }

                    ActualPosition += new Vector2(x, y);

                    Console.WriteLine(ActualPosition);
                }

                if (_isDirty)
                {
                    _matrix = Matrix.CreateTranslation(new Vector3(-ActualPosition.X, -ActualPosition.Y, 0)) *
                              Matrix.CreateRotationZ(_rotation) * Matrix.CreateScale(new Vector3(_zoom, _zoom, 1)) *
                              Matrix.CreateTranslation(new Vector3(Game.Core.Resolution.X * 0.5f, Game.Core.Resolution.Y * 0.5f, 0));
                }

                return _matrix;
            }
        }
    }
}
