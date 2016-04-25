namespace Alex
{
    public struct Settings
    {
        public string Username { get; set; }
        public int RenderDistance { get; set; }

        public Settings(string username)
        {
            Username = username;
            RenderDistance = 12;
        }
    }
}
