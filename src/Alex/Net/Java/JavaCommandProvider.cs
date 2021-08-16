using System.Threading;
using Alex.Common.Commands.Nodes;
using Alex.Networking.Java;
using Alex.Networking.Java.Packets.Play;
using Alex.Utils.Commands;
using Alex.Worlds;
using Alex.Worlds.Multiplayer;

namespace Alex.Net.Java
{
	public class JavaCommandProvider : CommandProvider
	{
		private JavaWorldProvider Provider { get; }
		private NetConnection Connection { get; }

		public JavaCommandProvider(JavaWorldProvider provider, NetConnection connection, World world) : base(world)
		{
			Provider = provider;
			Connection = connection;
		}
		
		private int _transactionIds = 0;
		private OnCommandMatch _callback = null;
		/// <inheritdoc />
		public override void DoMatch(string input, OnCommandMatch callback)
		{
			_callback = callback;
			var transactionId = Interlocked.Increment(ref _transactionIds);
			var packet = TabCompleteServerBound.CreateObject();
			packet.TransactionId = transactionId;
			packet.Text = input;
			
			Connection.SendPacket(packet);
		}

		public void HandleTabCompleteClientBound(TabCompleteClientBound tabComplete)
		{
			if (tabComplete.TransactionId == _transactionIds)
			{
				_callback?.Invoke(tabComplete.Start, tabComplete.Length, tabComplete.Matches);
			}
		}
	}
}