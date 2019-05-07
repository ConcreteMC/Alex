using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Alex.API.Data
{
    public class SavedServers
    {
	    public IReadOnlyList<SavedServer> Entries => Servers.ToArray();
		
		private List<SavedServer> Servers { get; set; }
		public SavedServers()
	    {

	    }

	    public void AddServer(SavedServer server)
	    {
			Servers.Add(server);
	    }

	    public void RemoveServer(SavedServer server)
	    {
		    Servers.Remove(server);
	    }

	    public static SavedServers ReadFromStream(Stream stream, bool keepOpen = false)
	    {
			SavedServers servers = new SavedServers();
		    using (BinaryReader reader = new BinaryReader(stream, Encoding.Unicode, keepOpen))
		    {
				uint count = reader.ReadUInt32();
			    for (int i = 0; i < count; i++)
			    {
					SavedServer s = SavedServer.ReadFrom(reader);
					servers.Servers.Add(s);
			    }
		    }

		    return servers;
	    }

	    public void WriteToStream(Stream stream, bool keepOpen = false)
	    {
		    var servers = Servers.ToArray();
		    using (BinaryWriter w = new BinaryWriter(stream, Encoding.Unicode, keepOpen))
		    {
			    w.Write((uint)servers.Length);
			    for (int i = 0; i < servers.Length; i++)
			    {
				    servers[i].WriteTo(w);
			    }
		    }
	    }

		public class SavedServer
	    {
			public string Host { get; set; }
			public ushort Port { get; set; }

		    private uint[] Icon { get; set; } = null;
		    private int Height { get; set; } = 0;
		    private int Width { get; set; } = 0;

		    public bool HasIcon { get; private set; } = false;

		    public bool IsBedrock { get; set; } = false;
		    public SavedServer(string host, ushort port)
		    {
			    Host = host;
			    Port = port;
		    }

		    private SavedServer()
		    {

		    }
	
		    internal static SavedServer ReadFrom(BinaryReader stream)
		    {
			    SavedServer s = new SavedServer();
				s.Host = stream.ReadString();
				s.Port = stream.ReadUInt16();
				s.IsBedrock = stream.ReadBoolean();

			    s.Height = stream.ReadInt32();
			    s.Width = stream.ReadInt32();

			    var dataSize = s.Width * s.Height;

				uint[] data = new uint[dataSize];
			    for (int i = 0; i < dataSize; i++)
			    {
				    data[i] = stream.ReadUInt32();
			    }

			    s.Icon = data;
			    s.HasIcon = data.Length > 0;

			    return s;
		    }

		    internal void WriteTo(BinaryWriter stream)
		    {
				stream.Write(Host);
				stream.Write(Port);
				stream.Write(IsBedrock);

				stream.Write(Height);
				stream.Write(Width);
			    for (int i = 0; i < Icon.Length; i++)
			    {
					stream.Write(Icon[i]);
			    }
				
		    }

			public bool GetIcon(GraphicsDevice device, out Texture2D icon)
		    {
			    if (!HasIcon)
			    {
				    icon = null;
				    return false;
			    }

				icon = new Texture2D(device, Width, Height);
				icon.SetData(Icon);
			    return true;
		    }

		    public void SetIcon(Texture2D icon)
		    {
			    uint[] i = new uint[icon.Width * icon.Height];
				icon.GetData(i);

			    HasIcon = true;
			    Icon = i;
		    }
	    }
    }
}
