namespace Alex.Entities
{
    public class Player : PlayerMob
	{
        public static readonly float EyeLevel = 1.625F;

	    public Player(string name) : base(name, null)
	    {
		   // Model = "geometry.humanoid";
			
	    }
    }
}