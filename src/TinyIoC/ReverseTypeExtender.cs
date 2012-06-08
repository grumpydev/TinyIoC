using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Reflection
{
	public static class ReverseTypeExtender
	{
		public static bool IsClass(this Type type)
		{
			return type.IsClass;
		}

		public static bool IsAbstract(this Type type)
		{
			return type.IsAbstract;
		}

		public static bool IsInterface(this Type type)
		{
			return type.IsInterface;
		}

		public static bool IsPrimitive(this Type type)
		{
			return type.IsPrimitive;
		}

		public static bool IsValueType(this Type type)
		{
			return type.IsValueType;
		}

		public static bool IsGenericType(this Type type)
		{
			return type.IsGenericType;
		}

		public static bool IsGenericParameter(this Type type)
		{
			return type.IsGenericParameter;
		}

		public static bool IsGenericTypeDefinition(this Type type)
		{
			return type.IsGenericTypeDefinition;
		}

		public static Type BaseType(this Type type)
		{
			return type.BaseType;
		}

		public static Assembly Assembly(this Type type)
		{
			return type.Assembly;
		}
	}
}
