using Alex.MoLang.Runtime.Value;

namespace Alex.MoLang.Runtime.Struct
{
	public interface IMoStruct : IMoValue
	{
		void Set(string key, IMoValue value);

		IMoValue Get(string key, MoParams parameters);

		void Clear();

		/// <inheritdoc />
		bool IMoValue.Equals(IMoValue b)
		{
			return this.Equals((object)b);
		}
	}
}