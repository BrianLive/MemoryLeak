using System;
using Microsoft.Xna.Framework;

namespace MemoryLeak.Core
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
				_zoom = (_zoom < 0.1f ? 0.1f : value);
				_isDirty = true;
			}
		}

		public Matrix Matrix
		{
			get
			{
				if (_isDirty)
				{
                    _matrix = Matrix.CreateTranslation(new Vector3(-(float)Math.Round(_position.X), -(float)Math.Round(_position.Y), 0)) *
							  Matrix.CreateRotationZ(_rotation) * Matrix.CreateScale(new Vector3(_zoom, _zoom, 1)) *
							  Matrix.CreateTranslation(new Vector3((float)Math.Round(Game.Core.Resolution.X * 0.5f), (float)Math.Round(Game.Core.Resolution.Y * 0.5f), 0));
				}

				return _matrix;
			}
		}
	}
}
