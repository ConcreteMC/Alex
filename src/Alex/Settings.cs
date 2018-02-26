namespace Alex
{
    public struct Settings
    {
        public string Username { get; set; }
        public int RenderDistance { get; set; }
        public double MouseSensitivy { get; set; }
		public string[] ResourcePacks { get; set; }

	    public string Anvil { get; set; }
		public bool UseBuiltinGenerator { get; set; }

        public Settings(string username)
        {
            Username = username;
	        ResourcePacks = new string[]
	        {
				
	        };
	        RenderDistance = 6;
            MouseSensitivy = 1.0;

	        Anvil = string.Empty;
	        UseBuiltinGenerator = false;
        }
    }
}
