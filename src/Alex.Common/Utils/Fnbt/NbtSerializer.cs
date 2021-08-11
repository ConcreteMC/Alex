using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using fNbt;

namespace Alex.Common.Utils.Fnbt
{
	public static class NbtSerializer
	{
		private const BindingFlags MemberBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		public static NbtCompound SerializeObject(object value)
		{
			return (NbtCompound)SerializeChild(null, value);
		}

		public static T DeserializeObject<T>(NbtTag tag)
		{
			return (T)DeserializeChild(typeof(T), tag);
		}

		public static void FillObject<T>(T value, NbtTag tag) where T : class
		{
			FillObject(value, value.GetType(), tag);
		}

		private static NbtTag SerializeChild(string name, object value)
		{
			if (value is NbtTag normalValue)
			{
				normalValue = (NbtTag)normalValue.Clone();
				normalValue.Name = name;
				return normalValue;
			}

			var tag = CreateBaseTag(name, value);
			if (tag != null) return tag;

			if (value is IList list)
			{
				return GetNbtList(name, list);
			}
			else if (value is IDictionary dictionary)
			{
				return GetNbtCompound(name, dictionary);
			}

			var type = value.GetType();

			var properties = type.GetProperties(MemberBindingFlags);
			var fields = type.GetFields(MemberBindingFlags);

			if (properties.Length == 0 && fields.Length == 0) return null;

			var nbt = new NbtCompound();
			if (name != null) nbt.Name = name;

			foreach (var property in properties)
			{
				var child = SerializeMember(property, property.GetValue(value));
				if (child != null) nbt.Add(child);
			}

			foreach (var filed in fields)
			{
				var child = SerializeMember(filed, filed.GetValue(value));
				if (child != null) nbt.Add(child);
			}

			if (nbt.Count == 0) return null;

			return nbt;
		}

		private static NbtTag SerializeMember(MemberInfo memberInfo, object value)
		{
			var attribute = GetAttribute(memberInfo);
			if (attribute == null) return null;

			if (attribute.HideDefault && value.Equals(GetDefaultValue(value))) return null;

			string childName = attribute.Name ?? memberInfo.Name;
			return SerializeChild(childName, value);
		}

		public static object GetDefaultValue(object value)
		{
			var type = value.GetType();

			if (type == typeof(byte) || type == typeof(sbyte) ||
				type == typeof(short) || type == typeof(ushort) ||
				type == typeof(int) || type == typeof(uint) ||
				type == typeof(long) || type == typeof(ulong) ||
				type == typeof(double) || type == typeof(float))
				return 0;
			else if (type == typeof(bool))
				return false;

			return null;
		}

		private static object DeserializeChild(Type type, NbtTag tag)
		{
			if (typeof(NbtTag).IsAssignableFrom((Type?)type))
			{
				tag = (NbtTag)tag.Clone();
				tag.Name = null;
				return tag;
			}

			var value = GetValueFromTag(tag, type);
			if (value != null) return value;

			if (typeof(IList).IsAssignableFrom(type))
			{
				return GetList(type, (NbtList)tag);
			}
			else if (typeof(IDictionary).IsAssignableFrom(type))
			{
				return GetDictionary(type, (NbtCompound)tag);
			}

			value = Activator.CreateInstance(type);

			DeserializeBase(value, type, tag);

			return value;
		}

		private static void DeserializeBase(object value, Type type, NbtTag tag)
		{
			var compound = (NbtCompound)tag;

			var properties = type.GetProperties();
			var fields = type.GetFields();

			if (compound.Count == 0) return;

			foreach (var property in properties)
			{
				if (!TryGetMemberTag(property, compound, out NbtTag child)) continue;

				if (property.SetMethod == null)
				{
					FillObject(property.GetValue(value), property.PropertyType, child);
					continue;
				}

				property.SetValue(value, DeserializeChild(property.PropertyType, child));
			}

			foreach (var filed in fields)
			{
				if (!TryGetMemberTag(filed, compound, out NbtTag child)) continue;
				filed.SetValue(value, DeserializeChild(filed.FieldType, child));
			}
		}

		private static bool TryGetMemberTag(MemberInfo memberInfo, NbtCompound compound, out NbtTag tag)
		{
			tag = null;

			var attribute = GetAttribute(memberInfo);
			if (attribute == null) return false;

			string childName = attribute.Name ?? memberInfo.Name;
			return compound.TryGet(childName, out tag);
		}

		private static void FillObject(object value, Type type, NbtTag tag)
		{
			var baseTypeValue = GetValueFromTag(tag, type);
			if (baseTypeValue != null) return;

			if (value is IList list)
			{
				list.Clear();
				FillList(list, list.GetType(), (NbtList)tag);
				return;
			}
			else if (value is IDictionary dictionary)
			{
				dictionary.Clear();
				FillDictionary(dictionary, dictionary.GetType(), (NbtCompound)tag);
				return;
			}

			DeserializeBase(value, type, tag);
		}

		private static NbtPropertyAttribute GetAttribute(MemberInfo memberInfo)
		{
			return memberInfo.GetCustomAttribute<NbtPropertyAttribute>();
		}

		private static NbtTag CreateBaseTag(string name, object value)
		{
			var type = value.GetType();

			if (type == typeof(byte) || type == typeof(sbyte) || type == typeof(bool))
				return new NbtByte(name, Convert.ToByte(value));
			else if (type == typeof(short) || type == typeof(ushort))
				return new NbtShort(name, Convert.ToInt16(value));
			else if (type == typeof(int) || type == typeof(uint))
				return new NbtInt(name, Convert.ToInt32(value));
			else if (type == typeof(long) || type == typeof(ulong))
				return new NbtLong(name, Convert.ToInt64(value));
			else if (type == typeof(double))
				return new NbtDouble(name, (double)value);
			else if (type == typeof(float))
				return new NbtFloat(name, (float)value);
			else if (type == typeof(string))
				return new NbtString(name, (string)value);
			else if (type == typeof(byte[]))
				return new NbtByteArray(name, (byte[])value);
			else if (type == typeof(int[]))
				return new NbtIntArray(name, (int[])value);

			return null;
		}

		private static object GetValueFromTag(NbtTag tag, Type type)
		{
			switch (tag)
			{
				case NbtByte value:
					if (type == typeof(bool))
						return Convert.ToBoolean(value.Value);
					else if (type == typeof(sbyte))
						return (sbyte)value.Value;
					return value.Value;
				case NbtShort value:
					if (type == typeof(ushort))
						return (ushort)value.Value;
					return value.Value;
				case NbtInt value:
					if (type == typeof(uint))
						return (uint)value.Value;
					return value.Value;
				case NbtLong value:
					if (type == typeof(ulong))
						return (ulong)value.Value;
					return value.Value;
				case NbtDouble value: return value.Value;
				case NbtFloat value: return value.Value;
				case NbtString value: return value.Value;
				case NbtByteArray value: return value.Value;
				case NbtIntArray value: return value.Value;
				default: return null;
			};
		}

		private static NbtList GetNbtList(string name, IList list)
		{
			if (list.Count == 0) return null;

			var nbt = new NbtList();
			if (name != null) nbt.Name = name;

			foreach (var value in list)
			{
				nbt.Add(SerializeChild(null, value));
			}

			return nbt;
		}

		private static IList GetList(Type type, NbtList tag)
		{
			var list = (IList)Activator.CreateInstance(type);

			FillList(list, type, tag);
			return list;
		}

		private static void FillList(IList list, Type type, NbtList tag)
		{
			if (tag.Count == 0) return;

			var listType = type.GetGenericArguments().First();

			foreach (var child in tag)
			{
				list.Add(DeserializeChild(listType, child));
			}
		}

		private static NbtCompound GetNbtCompound(string name, IDictionary dictionary)
		{
			if (dictionary.Count == 0) return null;
			if (dictionary.GetType().GetGenericArguments().First() != typeof(string)) return null;

			var keys = dictionary.Keys.GetEnumerator();
			var values = dictionary.Values.GetEnumerator();

			var nbt = new NbtCompound();
			if (name != null) nbt.Name = name;

			while (keys.MoveNext() && values.MoveNext())
			{
				var childName = (string)keys.Current;
				nbt.Add(SerializeChild(childName, values.Current));
			}

			return nbt;
		}

		private static IDictionary GetDictionary(Type type, NbtCompound tag)
		{
			var dictionary = (IDictionary)Activator.CreateInstance(type);

			FillDictionary(dictionary, type, tag);
			return dictionary;
		}

		private static void FillDictionary(IDictionary dictionary, Type type, NbtCompound tag)
		{
			var dictionaryType = type.GetGenericArguments().Last();

			foreach (var child in tag)
			{
				dictionary.Add(child.Name, DeserializeChild(dictionaryType, child));
			}
		}
	}
}