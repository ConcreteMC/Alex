namespace Alex.CoreRT.API.World
{
	public interface IBlock
	{
		uint BlockStateID { get; }
		int BlockId { get; }
		byte Metadata { get; }
		bool Solid { get; set; }
		bool Transparent { get; set; }
		bool Renderable { get; set; }
		bool HasHitbox { get; set; }
		float Drag { get; set; }
		string DisplayName { get; set; }
	}
}
