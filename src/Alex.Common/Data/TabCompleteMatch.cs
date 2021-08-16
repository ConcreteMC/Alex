using System;

namespace Alex.Common.Data
{
	public class TabCompleteMatch : IEquatable<TabCompleteMatch>
	{
		public string Match;
		
		public bool   HasTooltip = false;
		public string Tooltip    = null;

		public string Description = null;

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

			return Equals((TabCompleteMatch) obj);
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
