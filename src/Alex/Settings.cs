namespace Alex
{
    public struct Settings
    {
        public string Username { get; set; }
        public int RenderDistance { get; set; }
        public double MouseSensitivy { get; set; }
		public string[] ResourcePacks { get; set; }

        public Settings(string username)
        {
            Username = username;
	        ResourcePacks = new string[]
	        {
				
	        };
	        RenderDistance = 6;
            MouseSensitivy = 1.0;
        }
    }
}
