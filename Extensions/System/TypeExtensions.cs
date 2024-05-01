using System;
using System.Reflection;
using Object = System.Object;

namespace Editor.Hierarchy.Extensions.System
{
	internal static class TypeExtensions
	{
		public const BindingFlags FullBinding = (BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

		private const BindingFlags _instanceBinding = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		private const BindingFlags _staticBinding = (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

		internal static FieldInfo FindField(this Type type, String fieldName, BindingFlags flags = FullBinding)
		{
			return type.GetField(fieldName, flags);
		}

		internal static PropertyInfo FindProperty(this Type type, String propertyName, BindingFlags flags = FullBinding)
		{
			return type.GetProperty(propertyName, flags);
		}

		internal static MethodInfo FindMethod(this Type type, String methodName, Type[] argsTypes = null, BindingFlags flags = FullBinding)
		{
			return argsTypes == null ? type.GetMethod(methodName, flags) : type.GetMethod(methodName, flags, null, argsTypes, null);
		}

		private static Object RawCall(Type type, Object systemObject, String methodName, Object[] arguments, Type[] argumentsTypes, Boolean isStatic)
		{
			if ((systemObject == null) && (!isStatic))
			{
				throw new ArgumentNullException(nameof(systemObject), "obj cannot be null for instance methods");
			}

			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			for (var argumentTypeIndex = 0; argumentTypeIndex < argumentsTypes.Length; argumentTypeIndex++)
			{
				if (argumentsTypes[argumentTypeIndex] == typeof(Object))
				{
					argumentsTypes[argumentTypeIndex] = arguments[argumentTypeIndex].GetType();
				}
			}

			var methodInfo = type.FindMethod(methodName, argumentsTypes, (isStatic ? _staticBinding : _instanceBinding));

			if (methodInfo is null)
			{
				throw new MissingMethodException(type.FullName, methodName);
			}

			return methodInfo.Invoke(systemObject, arguments);
		}

		internal static TObject GetStaticField<TObject>(this Type type, String fieldName)
		{
			if (type is null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return (TObject)type.FindField(fieldName, _staticBinding).GetValue(null);
		}

		internal static void SetStaticField<TValue>(this Type type, String fieldName, TValue value)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			type.FindField(fieldName, _staticBinding).SetValue(null, value);
		}

		internal static Boolean HasField(this Type type, String fieldName)
		{
			return type.FindField(fieldName) != null;
		}

		internal static void InvokeStaticMethod(this Type type, String methodName)
		{
			var arguments = Array.Empty<Object>();
			var argumentsTypes = Type.EmptyTypes;

			RawCall(type, null, methodName, arguments, argumentsTypes, true);
		}

		internal static void InvokeStaticMethod<TArgument>(this Type type, String methodName, TArgument argument)
		{
			var arguments = new Object[]
			{
				argument,
			};

			var argsTypes = new Type[]
			{
				typeof(TArgument),
			};

			RawCall(type, null, methodName, arguments, argsTypes, true);
		}

		internal static TResult InvokeStaticMethod<TResult, TFirstArgument, TSecondArgument, TThirdArgument>(this Type type, String methodName, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			var arguments = new Object[]
			{
				firstArgument,
				secondArgument,
				thirdArgument,
			};

			var argsTypes = new Type[]
			{
				typeof(TFirstArgument),
				typeof(TSecondArgument),
				typeof(TThirdArgument),
			};

			return (TResult)RawCall(type, null, methodName, arguments, argsTypes, true);
		}

		internal static void InvokeStaticMethod<TFirstArgument, TSecondArgument>(this Type type, String methodName, TFirstArgument firstArgument, TSecondArgument secondArgument)
		{
			var args = new Object[]
			{
				firstArgument,
				secondArgument,
			};

			var argsTypes = new Type[]
			{
				typeof(TFirstArgument),
				typeof(TSecondArgument),
			};

			RawCall(type, null, methodName, args, argsTypes, true);
		}

		internal static TResult InvokeStaticMethod<TResult, TArgument>(this Type type, String methodName, TArgument argument)
		{
			var arguments = new Object[] { argument };

			var argumentsTypes = new Type[]
			{
				typeof(TArgument),
			};

			return (TResult)RawCall(type, null, methodName, arguments, argumentsTypes, true);
		}

		internal static TResult InvokeStaticMethod<TResult>(this Type type, String methodName)
		{
			var arguments = Array.Empty<Object>();
			var argumentsTypes = Type.EmptyTypes;

			return (TResult)RawCall(type, null, methodName, arguments, argumentsTypes, true);
		}

		internal static Boolean HasMethod<TFirstArgument, TSecondArgument, TThirdArgument>(this Type type, String methodName)
		{
			var argumentTypes = new Type[]
			{
				typeof(TFirstArgument),
				typeof(TSecondArgument),
				typeof(TThirdArgument),
			};

			return type.FindMethod(methodName, argumentTypes) != null;
		}
	}
}