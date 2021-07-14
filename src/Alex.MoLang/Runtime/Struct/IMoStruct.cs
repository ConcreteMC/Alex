using Alex.MoLang.Runtime.Value;
using Alex.MoLang.Utils;

namespace Alex.MoLang.Runtime.Struct
{
	public interface IMoStruct : IMoValue
	{
		void Set(MoPath key, IMoValue value);

		IMoValue Get(MoPath key, MoParams parameters);

		void Clear();

		/// <inheritdoc />
		bool IMoValue.Equals(IMoValue b)
		{
			return this.Equals((object)b);
		}
	}
}