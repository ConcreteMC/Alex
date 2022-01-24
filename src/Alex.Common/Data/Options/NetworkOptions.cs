using System;
using System.Runtime.Serialization;

namespace Alex.Common.Data.Options
{
	[DataContract]
	public class NetworkOptions : OptionsBase
	{
		[DataMember] public OptionsProperty<int> NetworkThreads { get; set; }

		[DataMember] public OptionsProperty<long> InactivityTimeout { get; set; }

		[DataMember] public OptionsProperty<int> ResendThreshold { get; set; }

		public NetworkOptions()
		{
			NetworkThreads = DefineRangedProperty(Environment.ProcessorCount / 2, 1, Environment.ProcessorCount);
			InactivityTimeout = DefineProperty(8500L, (value, newValue) => newValue);
			ResendThreshold = DefineProperty(10, (value, newValue) => newValue);
		}
	}
}