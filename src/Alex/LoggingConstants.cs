namespace Alex
{
	public class LoggingConstants
	{
		/// <summary>
		///		If true, logs unimplemented entity flags to the console
		/// </summary>
		public const bool LogUnimplementedEntityFlags = false;

		/// <summary>
		///		If true, reports bedrock network statistics to console every second
		/// </summary>
		/// 
#if DEBUG
		public const bool LogNetworkStatistics = true;
#else
		public const bool LogNetworkStatistics = false;
#endif

		/// <summary>
		///		If true, report invalid blockstate properties in debug console
		/// </summary>
		public const bool LogInvalidBlockProperties = false;

		/// <summary>
		///		If true, log server supported entities to console
		/// </summary>
		public const bool LogServerEntityDefinitions = false;

		public const bool LogUnknownParticles = false;
		
		public const bool LogUnknownEntityAttributes = false;
	}
}