using System;
using Microsoft.Xna.Framework;

namespace MemoryLeak.Graphics
{
	public class Camera
	{
		private Matrix _matrix;
		private Vector2 _position = Vector2.Zero;
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
			    _zoom = Math.Max(value, 0.1f);
				_isDirty = true;
			}
		}

		public Matrix Matrix
		{
			get
			{
				if (_isDirty)
				{
				    Matrix position = Matrix.CreateTranslation((float)Math.Round(-_position.X), (float)Math.Round(-_position.Y), 0);
				    Matrix rotation = Matrix.CreateRotationZ(_rotation);
				    Matrix scale = Matrix.CreateScale(Zoom, Zoom, 1);
				    Matrix center = Matrix.CreateTranslation(Game.Core.Resolution.X / 2, Game.Core.Resolution.Y / 2, 0);

				    _matrix = position * rotation * scale * center;
				}

				return _matrix;
			}
		}
	}
}
