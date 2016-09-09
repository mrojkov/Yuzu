﻿using System;

using Yuzu.Util;

namespace Yuzu.Binary
{
	public class BinarySerializeOptions
	{
		public byte[] Signature = new byte[] { (byte)'Y', (byte)'B', (byte)'0', (byte)'1' };
		public bool AutoSignature = false;
	}

	// These values are part of format.
	public enum RoughType : byte
	{
		None = 0,
		SByte = 1,
		Byte = 2,
		Short = 3,
		UShort = 4,
		Int = 5,
		UInt = 6,
		Long = 7,
		ULong = 8,
		Bool = 9,
		Char = 10,
		Float = 11,
		Double = 12,
		Decimal = 13,
		DateTime = 14,
		TimeSpan = 15,
		String = 16,
		Any = 17,
		Nullable = 18,

		Record = 32,
		Sequence = 33,
		Mapping = 34,

		FirstAtom = SByte,
		LastAtom = Any,
	}

	internal static class RT
	{
		public static Type[] roughTypeToType = new Type[] {
			null,
			typeof(sbyte), typeof(byte), typeof(short), typeof(ushort),
			typeof(int), typeof(uint), typeof(long), typeof(ulong),
			typeof(bool), typeof(char), typeof(float), typeof(double), typeof(decimal),
			typeof(DateTime), typeof(TimeSpan), typeof(string),
			typeof(object),
		};

		public static bool IsRecord(this Type t)
		{
			return t.IsClass || t.IsInterface || Utils.IsStruct(t);
		}
	}

}