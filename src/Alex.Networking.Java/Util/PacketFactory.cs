using System;
using System.Collections.Generic;
using System.IO;
using Alex.Networking.Java.Framework;

namespace Alex.Networking.Java.Util
{
	public class PacketFactory<TType, TStream, TPacket>
		where TType : IComparable<TType> where TStream : Stream where TPacket : IPacket<TStream>
	{
		private Dictionary<Type, TType> IdMap { get; }
		private Dictionary<TType, Func<TPacket>> Packets { get; }

		private readonly object _addLock = new object();

		public PacketFactory()
		{
			IdMap = new Dictionary<Type, TType>();
			Packets = new Dictionary<TType, Func<TPacket>>();
		}

		public void Register(TType packetId, Func<TPacket> createPacket)
		{
			lock (_addLock)
			{
				if (Packets.ContainsKey(packetId))
				{
					throw new DuplicatePacketIdException<TType>(packetId);
				}

				Packets.Add(packetId, createPacket);
				IdMap.Add(createPacket().GetType(), packetId);
			}
		}

		public bool TryGetPacket(TType packetId, out TPacket packet)
		{
			Func<TPacket> p;

			if (!Packets.TryGetValue(packetId, out p))
			{
				packet = default(TPacket);

				return false;
			}

			packet = p();

			return true;
		}

		public bool TryGetPacketId(Type type, out TType id)
		{
			if (IdMap.TryGetValue(type, out id))
			{
				return true;
			}

			id = default(TType);

			return false;
		}

		public bool TryGetPacket<TPacketType>(out TPacketType packet) where TPacketType : TPacket
		{
			TType id;

			if (TryGetPacketId(typeof(TPacketType), out id))
			{
				packet = (TPacketType)Packets[id]();

				return true;
			}

			packet = default(TPacketType);

			return false;
		}
	}

	public class DuplicatePacketIdException<TType> : Exception where TType : IComparable<TType>
	{
		internal DuplicatePacketIdException(TType id) : base($"A packet with the id \"{id}\" already exists!") { }
	}
}