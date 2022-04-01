using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Alex.Networking.Java.Framework;
using Alex.Networking.Java.Util;
using Newtonsoft.Json;
using NLog;

namespace Alex.Networking.Java.Packets
{
	public abstract class Packet : IPacket<MinecraftStream>
	{
		public Stopwatch Stopwatch { get; } = new Stopwatch();

		public int PacketId { get; set; } = -1;

		public virtual void Decode(MinecraftStream stream) { }

		public virtual Task DecodeAsync(MinecraftStream stream)
		{
			Decode(stream);

			return Task.CompletedTask;
		}

		public virtual void Encode(MinecraftStream stream) { }

		public virtual Task EncodeAsync(MinecraftStream stream)
		{
			Encode(stream);

			return Task.CompletedTask;
		}

		public virtual bool IsPooled => false;

		public abstract void PutPool();

		public virtual void Reset()
		{
			ResetPacket();

			//_encodedMessage = null;
			//Bytes = null;
			Stopwatch.Restart();

			/*_writer?.Close();
			_reader?.Close();
			_buffer?.Close();
			_writer = null;
			_reader = null;
			_buffer = null;*/
		}

		protected virtual void ResetPacket() { }
	}

	public abstract class Packet<TPacket> : Packet, IDisposable where TPacket : Packet<TPacket>, new()
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(Packet));

		private static readonly PacketPool<TPacket> Pool = new PacketPool<TPacket>(() => new TPacket());
		private bool _isPermanent;
		private bool _isPooled;
		private long _referenceCounter;

		[JsonIgnore]
		/// <inheritdoc />
		public override bool IsPooled => _isPooled;

		[JsonIgnore]
		public long ReferenceCounter
		{
			get { return _referenceCounter; }
			set { _referenceCounter = value; }
		}


		public TPacket MarkPermanent(bool permanent = true)
		{
			if (!_isPooled) throw new Exception("Tried to make non pooled item permanent");
			_isPermanent = permanent;

			return (TPacket)this;
		}

		public TPacket AddReferences(long numberOfReferences)
		{
			if (_isPermanent) return (TPacket)this;

			if (!_isPooled) throw new Exception("Tried to reference count a non pooled item");
			Interlocked.Add(ref _referenceCounter, numberOfReferences);

			return (TPacket)this;
		}

		public TPacket AddReference(Packet<TPacket> item)
		{
			if (_isPermanent) return (TPacket)this;

			if (!item.IsPooled) throw new Exception("Item template needs to come from a pool");

			Interlocked.Increment(ref item._referenceCounter);

			return (TPacket)item;
		}

		public TPacket MakePoolable(long numberOfReferences = 1)
		{
			_isPooled = true;
			_referenceCounter = numberOfReferences;

			return (TPacket)this;
		}


		public static TPacket CreateObject(long numberOfReferences = 1)
		{
			TPacket item = Pool.GetObject();
			item._isPooled = true;
			item._referenceCounter = numberOfReferences;
			item.Stopwatch.Restart();

			return item;
		}

		public override void PutPool()
		{
			if (_isPermanent) return;

			if (!IsPooled)
			{
				Log.Warn(
					$"Tried pooling non-pooled packet 0x{PacketId:x2} {GetType().Name}, IsPooled={_isPooled}, IsPermanent={_isPermanent}, Refs={_referenceCounter}");

				return;
			}

			long counter = Interlocked.Decrement(ref _referenceCounter);

			if (counter > 0) return;

			if (counter < 0)
			{
				Log.Error(
					$"Pooling error. Added pooled object too many times. 0x{PacketId:x2} {GetType().Name}, IsPooled={IsPooled}, IsPooled={_isPermanent}, Refs={_referenceCounter}");

				return;
			}

			Reset();

			_isPooled = false;

			//Pool.PutObject((T) this);
		}

		public void Dispose()
		{
			PutPool();
		}
	}
}