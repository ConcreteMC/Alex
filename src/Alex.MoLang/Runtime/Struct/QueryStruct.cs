using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;
using NLog;

namespace Alex.MoLang.Runtime.Struct
{
	public class QueryStruct : IMoStruct
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(QueryStruct));
		protected IDictionary<string, Func<MoParams, object>> Functions = new Dictionary<string, Func<MoParams, object>>();
		
		private static ConcurrentDictionary<string, int> _missingQueries = new ConcurrentDictionary<string, int>();
		/// <inheritdoc />
		public object Value => this;

		public QueryStruct()
		{
			
		}

		public QueryStruct(IEnumerable<KeyValuePair<string, Func<MoParams, object>>> parameters)
		{
			Functions = new Dictionary<string, Func<MoParams, object>>(parameters);
		}

		/// <inheritdoc />
		public void Set(string key, IMoValue value)
		{
			throw new NotSupportedException("Cannot set a value in a query struct.");
		}

		/// <inheritdoc />
		public IMoValue Get(string key, MoParams parameters)
		{
			if (Functions.TryGetValue(key, out var func))
			{
				return MoValue.FromObject(func(parameters));
			}

			//throw new MoLangRuntimeException($"Missing method: \'{key}\'", null);
			if (_missingQueries.TryAdd(key, 0))
			{
				Log.Warn($"Unknown query: query.{key}");
			}
			return DoubleValue.Zero;
		}

		private int AddValueFactory(string key)
		{
			return 1;
		}

		private int UpdateValueFactory(string key, int count)
		{
			return count + 1;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Functions.Clear();
		}
	}
}