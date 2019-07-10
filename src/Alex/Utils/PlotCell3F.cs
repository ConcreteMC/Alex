using System;
using Microsoft.Xna.Framework;

namespace Alex.Utils
{
	public class PlotCell3f
	{
		private Vector3 _delta;
		private Vector3 _dir;

		private Vector3 _index;

		private int _limit;
		private Vector3 _max;
		private int _plotted;
		private Vector3 _pos;
		private Vector3 _sign;

		private Vector3 _size;

		public PlotCell3f(Vector3 blocksize)
		{
			_size = blocksize;
		}

		public void Plot(Vector3 position, Vector3 direction, int cells)
		{
			_limit = cells;

			_pos = position;
			_dir = direction;
			_dir.Normalize();

			_delta = _size;
			_delta = _delta / _dir;

			_sign.X = (_dir.X > 0) ? 1 : (_dir.X < 0 ? -1 : 0);
			_sign.Y = (_dir.Y > 0) ? 1 : (_dir.Y < 0 ? -1 : 0);
			_sign.Z = (_dir.Z > 0) ? 1 : (_dir.Z < 0 ? -1 : 0);

			Reset();
		}

		public bool Next()
		{
			if (_plotted++ > 0)
			{
				var mx = _sign.X * _max.X;
				var my = _sign.Y * _max.Y;
				var mz = _sign.Z * _max.Z;

				if (mx < my && mx < mz)
				{
					_max.X += _delta.X;
					_index.X += _sign.X;
				}
				else if (mz < my && mz < mx)
				{
					_max.Z += _delta.Z;
					_index.Z += _sign.Z;
				}
				else
				{
					_max.Y += _delta.Y;
					_index.Y += _sign.Y;
				}
			}
			return (_plotted <= _limit);
		}

		public void Reset()
		{
			_plotted = 0;

			_index.X = (int)Math.Floor((_pos.X) / _size.X);
			_index.Y = (int)Math.Floor((_pos.Y) / _size.Y);
			_index.Z = (int)Math.Floor((_pos.Z) / _size.Z);

			var ax = _index.X * _size.X;
			var ay = _index.Y * _size.Y;
			var az = _index.Z * _size.Z;

			_max.X = (_sign.X > 0) ? ax + _size.X - _pos.X : _pos.X - ax;
			_max.Y = (_sign.X > 0) ? ay + _size.Y - _pos.Y : _pos.Y - ay;
			_max.Z = (_sign.X > 0) ? az + _size.Z - _pos.Z : _pos.Z - az;
			_max = _max / _dir;
			// max.Length(dir);
		}

		public void End()
		{
			_plotted = _limit + 1;
		}

		public Vector3 Get()
		{
			return _index;
		}

		public Vector3 Actual()
		{
			return new Vector3(_index.X * _size.X,
				_index.Y * _size.Y,
				_index.Z * _size.Z);
		}

		public Vector3 Position()
		{
			return _pos;
		}

		public Vector3 Direction()
		{
			return _dir;
		}
	}
}
