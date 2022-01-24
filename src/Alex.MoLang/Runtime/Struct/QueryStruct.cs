using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;
using NLog;

namespace Alex.MoLang.Runtime.Struct
{
	public class QueryStruct : IMoStruct
	{
		private static readonly Logger Log = LogManager.GetCurrentClassLogger(typeof(QueryStruct));

		protected readonly IDictionary<string, Func<MoParams, object>> Functions =
			new Dictionary<string, Func<MoParams, object>>(StringComparer.OrdinalIgnoreCase);

		//private static ConcurrentDictionary<string, int> _missingQueries = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		/// <inheritdoc />
		public object Value => this;

		public QueryStruct() { }

		public bool UseNLog = true;
		public bool EnableDebugOutput = false;

		public QueryStruct(IEnumerable<KeyValuePair<string, Func<MoParams, object>>> parameters)
		{
			Functions = new Dictionary<string, Func<MoParams, object>>(parameters);

			Functions.Add(
				"debug_output", mo =>
				{
					if (EnableDebugOutput)
					{
						StringBuilder sb = new StringBuilder();

						foreach (var param in mo.GetParams())
						{
							sb.Append($"{param.AsString()} ");
						}

						if (UseNLog)
							Log.Debug(sb.ToString());
						else
							Console.WriteLine(sb.ToString());
					}

					return DoubleValue.Zero;
				});
		}

		/// <inheritdoc />
		public void Set(MoPath key, IMoValue value)
		{
			throw new NotSupportedException("Cannot set a value in a query struct.");
		}

		/// <inheritdoc />
		public IMoValue Get(MoPath key, MoParams parameters)
		{
			if (Functions.TryGetValue(key.ToString(), out var func))
			{
				return MoValue.FromObject(func(parameters));
			}

			//throw new MissingQueryMethodException($"Missing method: \'{key}\'", null);
			//if (_missingQueries.TryAdd(key, 0))
			//{
			//	Log.Warn($"Unknown query: query.{key}");
			//}
			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Functions.Clear();
		}
	}
}