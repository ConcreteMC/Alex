using System;
using System.Linq;

namespace Alex.MoLang.Utils
{
	public class MoPath
	{
		public MoPath Root { get; }
		public MoPath Previous { get; }
		
		public string Path { get; }
		public string Segment { get; }

		public bool HasChildren => Segments.Length > 0;
		
		public MoPath(string path)
		{
			Previous = null;
			Root = this;
			Path = path;

			var segments = path.Split('.');
			Segment = segments[0];

			Segments = new MoPath[segments.Length - 1];

			if (Segments.Length == 0)
				return;

			MoPath previous = this;
			for (int i = 0; i < Segments.Length; i++)
			{
				var moPath = new MoPath(this, previous, string.Join('.', segments.Skip(i + 1)), segments[i + 1]);
				previous = moPath;
				Segments[i] = moPath;
			}

			for (int i = 0; i < Segments.Length; i++)
			{
				Segments[i].Segments = Segments.Skip(1).ToArray();
			}
		}

		private MoPath(MoPath root, MoPath parent, string path, string segment)
		{
			Root = root;
			Path = path;
			Previous = parent;
			Segment = segment;
			
			
		}
		
		public MoPath[] Segments { get; private set; }

		/// <inheritdoc />
		public override string ToString()
		{
			return Path;
		}

		public static implicit operator MoPath(string value)
		{
			return new MoPath(value);
		}
	}
}