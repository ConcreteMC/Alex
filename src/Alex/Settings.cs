using Newtonsoft.Json;

namespace Alex
{
    public sealed class Settings
    {
		[JsonIgnore]
	    internal bool IsDirty { get; set; }

        public string Username { get; set; }
        public double MouseSensitivy { get; set; }
		public string[] ResourcePacks { get; set; }
        public string PluginDirectory { get; set; }
	    public string Anvil { get; set; }
		public bool UseBuiltinGenerator { get; set; }
		
		public bool ClientSideLighting { get; set; }
		public bool UseAlexChunks { get; set; }
		
        public Settings(string username)
        {
            Username = username;
	        ResourcePacks = new string[]
	        {
				
	        };
	        MouseSensitivy = 1.0;

	        Anvil = string.Empty;
	        UseBuiltinGenerator = false;
	        IsDirty = false;
	        
	        ClientSideLighting = false;
	        UseAlexChunks = false;

            PluginDirectory = "";
        }
    }
}
