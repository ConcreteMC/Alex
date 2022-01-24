using System;
using System.Collections.Generic;
using System.Linq;
using Alex.MoLang.Parser;
using Alex.MoLang.Runtime.Exceptions;
using Alex.MoLang.Runtime.Struct;
using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime
{
	public class MoParams
	{
		public static readonly MoParams Empty = new MoParams(new IMoValue[0]);

		private readonly IMoValue[] _parameters;

		public MoParams(params IMoValue[] param)
		{
			_parameters = param;
		}

		public IMoValue Get(int index)
		{
			return _parameters[index];
		}

		public T Get<T>(int index)
		{
			IMoValue obj = _parameters[index];

			if (obj == null)
				throw new MoLangRuntimeException(
					$"MoParams: Expected parameter type of {typeof(T).Name} got null", null);

			if (obj?.GetType() == typeof(T))
			{
				return (T)obj;
			}
			else
			{
				throw new MoLangRuntimeException(
					"MoParams: Expected parameter type of " + typeof(T).Name + ", " + obj.GetType().Name + " given.",
					null);
			}
		}

		public bool Contains(int index)
		{
			return _parameters.Length >= index + 1;
		}

		public int GetInt(int index)
		{
			return (int)GetDouble(index);
		}

		public double GetDouble(int index)
		{
			return Get<DoubleValue>(index).Value;
		}

		public IMoStruct GetStruct(int index)
		{
			return Get<IMoStruct>(index);
		}

		public string GetString(int index)
		{
			return Get<StringValue>(index).Value;
		}

		public MoLangEnvironment GetEnv(int index)
		{
			return Get<MoLangEnvironment>(index);
		}

		public IMoValue[] GetParams()
		{
			return _parameters;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var values = GetParams();

			if (values != null && values.Length > 0)
			{
				return string.Join(',', values.Select(x => x.AsString()));
			}

			return base.ToString();
		}
	}
}