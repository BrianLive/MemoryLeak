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
		private Vector2 _velocity = Vector2.Zero;

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
				if (ActualPosition != _position)
				{
					const float acceleration = 0.25f;

					_velocity += LengthDir(Direction(ActualPosition, _position), acceleration);

					ActualPosition += _velocity;

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

		private static float Direction(Vector2 p1, Vector2 p2)
		{
			float r = (float)Math.Atan2(p1.Y - p2.Y, p2.X - p1.X);
			return r < 0 ? r + (2 * (float)Math.PI) : r;
		}

		private static Vector2 LengthDir(float dir, float len)
		{
			return new Vector2((float)Math.Cos(dir) * len, (float)-Math.Sin(dir) * len);
		}
	}
}
