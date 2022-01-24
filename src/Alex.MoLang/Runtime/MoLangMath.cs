using System;
using System.Collections.Generic;
using Alex.MoLang.Runtime.Struct;

namespace Alex.MoLang.Runtime
{
	public class MoLangMath
	{
		public static readonly QueryStruct Library = new QueryStruct(
			new Dictionary<string, Func<MoParams, object>>()
			{
				{ "abs", param => Math.Abs(param.GetDouble(0)) },
				{ "acos", param => Math.Acos(param.GetDouble(0)) },
				{ "sin", param => Math.Sin(param.GetDouble(0) * (Math.PI / 180d)) },
				{ "asin", param => Math.Asin(param.GetDouble(0)) },
				{ "atan", param => Math.Atan(param.GetDouble(0)) },
				{ "atan2", param => Math.Atan2(param.GetDouble(0), param.GetDouble(1)) },
				{ "ceil", param => Math.Ceiling(param.GetDouble(0)) },
				{
					"clamp", param => Math.Min(param.GetDouble(1), Math.Max(param.GetDouble(0), param.GetDouble(2)))
				},
				{ "cos", param => Math.Cos(param.GetDouble(0) * (Math.PI / 180d)) },
				{ "die_roll", param => DieRoll(param.GetDouble(0), param.GetDouble(1), param.GetDouble(2)) },
				{ "die_roll_integer", param => DieRollInt(param.GetInt(0), param.GetInt(1), param.GetInt(2)) },
				{ "exp", param => Math.Exp(param.GetDouble(0)) },
				{ "mod", param => param.GetDouble(0) % param.GetDouble(1) },
				{ "floor", param => Math.Floor(param.GetDouble(0)) },
				{ "hermite_blend", param => HermiteBlend(param.GetInt(0)) },
				{ "lerp", param => Lerp(param.GetDouble(0), param.GetDouble(1), param.GetDouble(2)) },
				{ "lerp_rotate", param => LerpRotate(param.GetDouble(0), param.GetDouble(1), param.GetDouble(2)) },
				{ "ln", param => Math.Log(param.GetDouble(0)) },
				{ "max", param => Math.Max(param.GetDouble(0), param.GetDouble(1)) },
				{ "min", param => Math.Min(param.GetDouble(0), param.GetDouble(1)) },
				{ "pi", param => Math.PI },
				{ "pow", param => Math.Pow(param.GetDouble(0), param.GetDouble(1)) },
				{ "random", param => Random(param.GetDouble(0), param.GetDouble(1)) },
				{ "random_integer", param => RandomInt(param.GetInt(0), param.GetInt(1)) },
				{ "round", param => Math.Round(param.GetDouble(0)) },
				{ "sqrt", param => Math.Sqrt(param.GetDouble(0)) },
				{ "trunc", param => Math.Floor(param.GetDouble(0)) },
			});

		public static double Random(double low, double high)
		{
			return low + _random.NextDouble() * (high - low);
		}

		private static Random _random = new Random();

		public static int RandomInt(int low, int high)
		{
			return _random.Next(low, high);
		}

		public static double DieRoll(double num, double low, double high)
		{
			int i = 0;
			double total = 0;
			while (i++ < num) total += Random(low, high);

			return total;
		}

		public static int DieRollInt(int num, int low, int high)
		{
			int i = 0;
			int total = 0;
			while (i++ < num) total += RandomInt(low, high);

			return total;
		}

		public static int HermiteBlend(int value)
		{
			return (3 * value) ^ (2 - 2 * value) ^ 3;
		}

		public static double Lerp(double start, double end, double amount)
		{
			amount = Math.Max(0, Math.Min(1, amount));

			return start + (end - start) * amount;
		}

		public static double LerpRotate(double start, double end, double amount)
		{
			start = Radify(start);
			end = Radify(end);

			if (start > end)
			{
				double tmp = start;
				start = end;
				end = tmp;
			}

			if (end - start > 180)
			{
				return Radify(end + amount * (360 - (end - start)));
			}

			return start + amount * (end - start);
		}

		public static double Radify(double num)
		{
			return (((num + 180) % 360) + 180) % 360;
		}
	}
}