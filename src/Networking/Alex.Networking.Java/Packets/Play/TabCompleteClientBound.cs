using System;
using Alex.Networking.Java.Util;

namespace Alex.Networking.Java.Packets.Play
{
	public class TabCompleteClientBound : Packet<TabCompleteClientBound>
	{
		public int TransactionId;
		public int Start;
		public int Length;

		public TabCompleteMatch[] Matches;

		public override void Decode(MinecraftStream stream)
		{
			TransactionId = stream.ReadVarInt();
			Start = stream.ReadVarInt();
			Length = stream.ReadVarInt();

			int c = stream.ReadVarInt();
			Matches = new TabCompleteMatch[c];

			for (int i = 0; i < c; i++)
			{
				var entry = new TabCompleteMatch();
				entry.Match = stream.ReadString();
				entry.HasTooltip = stream.ReadBool();

				if (entry.HasTooltip)
				{
					entry.Tooltip = stream.ReadString();
					//ChatObject.TryParse(stream.ReadString(), out entry.Tooltip);
				}

				Matches[i] = entry;
			}
		}

		public override void Encode(MinecraftStream stream)
		{
			throw new NotImplementedException();
		}
		
		public class TabCompleteMatch : IEquatable<TabCompleteMatch>
		{
			public string Match;

			public bool HasTooltip = false;
			public string Tooltip = null;

			public string Description = null;

			public TabCompleteMatch() { }

			public string GetDescriptive()
			{
				if (string.IsNullOrWhiteSpace(Description))
					return Match;

				return $"{Match} - {Description}";
			}

			/// <inheritdoc />
			public bool Equals(TabCompleteMatch other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;

				return Match == other.Match;
			}

			/// <inheritdoc />
			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj)) return false;
				if (ReferenceEquals(this, obj)) return true;
				if (obj.GetType() != this.GetType()) return false;

				return Equals((TabCompleteMatch)obj);
			}

			/// <inheritdoc />
			public override int GetHashCode()
			{
				return HashCode.Combine(Match, HasTooltip, Tooltip, Description);
			}

			/// <inheritdoc />
			public override string ToString()
			{
				return GetDescriptive();
			}
		}
	}
}