namespace Alex.Common.Entities.Properties
{
	public class Modifier
	{
		public MiNET.Utils.UUID Uuid;
		public double Amount;
		public ModifierMode Operation;

		public Modifier() { }

		public Modifier(MiNET.Utils.UUID uuid, double amount, ModifierMode mode)
		{
			Uuid = uuid;
			Amount = amount;
			Operation = mode;
		}
	}
}