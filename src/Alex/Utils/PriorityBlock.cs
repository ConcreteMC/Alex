using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Alex.Utils
{
	/// <summary>
	/// An IDisposable class that holds multiple IDisposables; used as the return value in PriorityBufferBlock.LinkTo.
	/// </summary>
	public sealed class DisposableMultiLink : IDisposable
	{
		private readonly IEnumerable<IDisposable> _contents;

		public DisposableMultiLink(params IDisposable[] items)
		{
			_contents = items;
		}

		public void Dispose()
		{
			foreach (var item in _contents)
				item?.Dispose();
		}
	}

	public enum Priority
	{
		High,
		Medium,
		Low
	}

	/// <summary>
	/// A TPL Dataflow-compatible BufferBlock supporting three priorities, Low, Medium, and High, and allowing the user to specify the DataflowBlockOptions for each internal buffer (eg., to set BoundingCapacity differently for each).
	/// The priority is declared when posting/sending, and that order is respected when a reservation is placed; if a higher priority item is received after a reservation has been provided, that reservation for the lower-priority item will be respected.
	/// Thus, it is possible that a lower-priority item will be returned even if there are higher-priority items in the buffer.
	/// The interface does not force this behavior, but strictly honoring the reservations was done to avoid potential reservation problems with the three internal BufferBlocks.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class PriorityBufferBlock<T> : ISourceBlock<T>, IReceivableSourceBlock<T>
	{
		private readonly BufferBlock<T> _highPriorityBuffer;
		private readonly BufferBlock<T> _mediumPriorityBuffer;
		private readonly BufferBlock<T> _lowPriorityBuffer;

		/// <summary>
		/// Holds all messages sent to this object.  Note that this collection is never emptied in this implementation, so its growth is unbounded.
		/// Because of the nature of this PriorityBufferBlock's use of composition, and because it's not clear when it's safe to discard old messages, that problem has been left.
		/// For instance, can a MessageHeader be discarded when either ReleaseReservation or ConsumeMessage is called, or is ConsumeMessage not the final link in the chain?
		/// </summary>
		private readonly ConcurrentDictionary<DataflowMessageHeader, ISourceBlock<T>> _messagesByBlock =
			new ConcurrentDictionary<DataflowMessageHeader, ISourceBlock<T>>();

		/// <summary>
		/// Each internal BufferBlock is built using the provided options specified for that priority.
		/// </summary>
		/// <param name="highPriorityOptions"></param>
		/// <param name="mediumPriorityOptions"></param>
		/// <param name="lowPriorityOptions"></param>
		public PriorityBufferBlock(DataflowBlockOptions highPriorityOptions,
			DataflowBlockOptions mediumPriorityOptions,
			DataflowBlockOptions lowPriorityOptions)
		{
			_highPriorityBuffer = new BufferBlock<T>(highPriorityOptions);
			_mediumPriorityBuffer = new BufferBlock<T>(mediumPriorityOptions);
			_lowPriorityBuffer = new BufferBlock<T>(lowPriorityOptions);
		}

		/// <summary>
		/// Builds the internal buffers using the provided options.
		/// </summary>
		/// <param name="options"></param>
		public PriorityBufferBlock(DataflowBlockOptions options) : this(options, options, options) { }

		/// <summary>
		/// Builds internal buffers with default BufferBlock options
		/// </summary>
		public PriorityBufferBlock()
		{
			_highPriorityBuffer = new BufferBlock<T>();
			_mediumPriorityBuffer = new BufferBlock<T>();
			_lowPriorityBuffer = new BufferBlock<T>();
		}

		public bool Any() => _highPriorityBuffer.Count > 0 || _mediumPriorityBuffer.Count > 0
		                                                   || _lowPriorityBuffer.Count > 0;

		/// <summary>
		/// Asynchronously adds an item to the buffer.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="priority"></param>
		/// <returns></returns>
		public Task SendAsync(T item, Priority priority)
		{
			switch (priority)
			{
				case Priority.High:
					return _highPriorityBuffer.SendAsync(item);

				case Priority.Medium:
					return _mediumPriorityBuffer.SendAsync(item);

				case Priority.Low:
					return _lowPriorityBuffer.SendAsync(item);

				default: throw new InvalidOperationException($"Priority {priority.ToString()} is invalid");
			}
		}

		/// <summary>
		/// Synchronously adds an item to the buffer; blocking.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="priority"></param>
		public void Post(T item, Priority priority)
		{
			switch (priority)
			{
				case Priority.High:
				{
					_highPriorityBuffer.Post(item);

					break;
				}

				case Priority.Medium:
				{
					_mediumPriorityBuffer.Post(item);

					break;
				}

				case Priority.Low:
				{
					_lowPriorityBuffer.Post(item);

					break;
				}

				default: throw new InvalidOperationException($"Priority {priority.ToString()} is invalid");
			}
		}

		public void MoveItems(PriorityBufferBlock<T> target)
		{
			if (_highPriorityBuffer.TryReceiveAll(out var highPriority))
			{
				foreach (var item in highPriority)
				{
					target._highPriorityBuffer.Post(item);
				}
			}

			if (_mediumPriorityBuffer.TryReceiveAll(out var midPrio))
			{
				foreach (var item in midPrio)
				{
					target._mediumPriorityBuffer.Post(item);
				}
			}

			if (_lowPriorityBuffer.TryReceiveAll(out var lowPrio))
			{
				foreach (var item in lowPrio)
				{
					target._lowPriorityBuffer.Post(item);
				}
			}
		}

		#region ISourceBlock methods

		public Task Completion => Task.WhenAll(
			_lowPriorityBuffer.Completion, _mediumPriorityBuffer.Completion, _highPriorityBuffer.Completion);

		public void Complete()
		{
			_lowPriorityBuffer.Complete();
			_mediumPriorityBuffer.Complete();
			_highPriorityBuffer.Complete();
		}

		T ISourceBlock<T>.ConsumeMessage(DataflowMessageHeader messageHeader,
			ITargetBlock<T> target,
			out bool messageConsumed)
		{
			if (_messagesByBlock.TryGetValue(messageHeader, out var block))
				return block.ConsumeMessage(messageHeader, target, out messageConsumed);
			else
				throw new InvalidOperationException("reservation not found for messageheader");
		}

		void IDataflowBlock.Fault(Exception exception)
		{
			(_lowPriorityBuffer as ISourceBlock<T>).Fault(exception);
			(_mediumPriorityBuffer as ISourceBlock<T>).Fault(exception);
			(_highPriorityBuffer as ISourceBlock<T>).Fault(exception);
		}

		public IDisposable LinkTo(ITargetBlock<T> target, DataflowLinkOptions linkOptions)
		{
			var l1 = _lowPriorityBuffer.LinkTo(target, linkOptions);
			var l2 = _mediumPriorityBuffer.LinkTo(target, linkOptions);
			var l3 = _highPriorityBuffer.LinkTo(target, linkOptions);

			return new DisposableMultiLink(l1, l2, l3);
		}

		void ISourceBlock<T>.ReleaseReservation(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			if (_messagesByBlock.TryGetValue(messageHeader, out var block))
				block.ReleaseReservation(messageHeader, target);
		}

		bool ISourceBlock<T>.ReserveMessage(DataflowMessageHeader messageHeader, ITargetBlock<T> target)
		{
			if (_highPriorityBuffer.Count > 0)
			{
				_messagesByBlock.TryAdd(messageHeader, _highPriorityBuffer);

				return (_highPriorityBuffer as ISourceBlock<T>).ReserveMessage(messageHeader, target);
			}
			else if (_mediumPriorityBuffer.Count > 0)
			{
				_messagesByBlock.TryAdd(messageHeader, _mediumPriorityBuffer);

				return (_mediumPriorityBuffer as ISourceBlock<T>).ReserveMessage(messageHeader, target);
			}
			else
			{
				_messagesByBlock.TryAdd(messageHeader, _lowPriorityBuffer);

				return (_lowPriorityBuffer as ISourceBlock<T>).ReserveMessage(messageHeader, target);
			}
		}

		#endregion

		#region IReceivableSourceBlock methods

		public bool TryReceive(Predicate<T> filter, out T item)
		{
			if (_highPriorityBuffer.Count > 0 && _highPriorityBuffer.TryReceive(filter, out item))
				return true;
			else if (_mediumPriorityBuffer.Count > 0 && _mediumPriorityBuffer.TryReceive(filter, out item))
				return true;
			else if (_lowPriorityBuffer.TryReceive(filter, out item))
				return true;
			else
			{
				item = default(T);

				return false;
			}
		}

		public bool TryReceiveAll(out IList<T> items)
		{
			var lowSuccess = _lowPriorityBuffer.TryReceiveAll(out var low);
			var medSuccess = _mediumPriorityBuffer.TryReceiveAll(out var medium);
			var highSuccess = _highPriorityBuffer.TryReceiveAll(out var high);
			var output = new List<T>();

			if (highSuccess)
				output.AddRange(high);

			if (medSuccess)
				output.AddRange(medium);

			if (lowSuccess)
				output.AddRange(low);

			items = output;

			return lowSuccess || medSuccess || highSuccess;
		}

		#endregion
	}

	public class PriorityBlock<T>
	{
		private readonly BufferBlock<T> _highPriorityTarget;

		public ITargetBlock<T> HighPriorityTarget
		{
			get { return _highPriorityTarget; }
		}

		private readonly BufferBlock<T> _lowPriorityTarget;

		public ITargetBlock<T> LowPriorityTarget
		{
			get { return _lowPriorityTarget; }
		}

		private readonly BufferBlock<T> _source;

		public ISourceBlock<T> Source
		{
			get { return _source; }
		}

		public PriorityBlock()
		{
			var options = new DataflowBlockOptions { BoundedCapacity = 1 };

			_highPriorityTarget = new BufferBlock<T>(options);
			_lowPriorityTarget = new BufferBlock<T>(options);
			_source = new BufferBlock<T>(options);

			Task.Run(() => ForwardMessages());
		}

		private async Task ForwardMessages()
		{
			while (true)
			{
				await Task.WhenAny(
					_highPriorityTarget.OutputAvailableAsync(), _lowPriorityTarget.OutputAvailableAsync());

				T item;

				if (_highPriorityTarget.TryReceive(out item))
				{
					await _source.SendAsync(item);
				}
				else if (_lowPriorityTarget.TryReceive(out item))
				{
					await _source.SendAsync(item);
				}
				else
				{
					// both input blocks must be completed
					_source.Complete();

					return;
				}
			}
		}
	}
}