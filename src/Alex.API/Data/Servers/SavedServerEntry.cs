namespace Alex.API.Data.Servers
{
    public class SavedServerEntry
    {
        public int ListIndex { get; set; }

        public ServerType ServerType { get; set; }

        public string Name { get; set; }

        public string Host { get; set; }
        public ushort Port { get; set; }

        public uint[] CachedIcon { get; set; }
    }
}
