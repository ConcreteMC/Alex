using System;
using System.Linq;
using System.Text;

namespace Alex.MoLang.Utils
{
	public class MoPath
	{
		public MoPath Root { get; }
		public MoPath Previous { get; }
		public MoPath Next { get; private set; }

		private readonly MoPath _last = null;
		public MoPath Last => _last ?? Root._last;

		public string Path { get; }
		public string Value { get; }

		public bool HasChildren => Next != null;

		public MoPath(string path)
		{
			Previous = null;
			Next = null;
			Root = this;
			Path = path;
			_last = this;

			var segments = path.Split('.');
			Value = segments[0];

			if (segments.Length > 1)
			{
				string currentPath = $"{Value}";

				for (int i = 1; i < segments.Length; i++)
				{
					var value = segments[i];

					if (string.IsNullOrWhiteSpace(value))
						break;

					currentPath += $".{value}";

					var moPath = new MoPath(Root, _last, currentPath, value);
					_last.Next = moPath;

					_last = moPath;
				}
			}
		}

		private MoPath(MoPath root, MoPath parent, string path, string value)
		{
			Root = root;
			Path = path;
			Previous = parent;
			Value = value;
		}

		//	public MoPath[] Segments { get; private set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return Value;
		}

		//public static implicit operator MoPath(string value)
		//{
		//	return new MoPath(value);
		//}
	}
}