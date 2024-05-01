using System;
using System.Reflection;

namespace Editor.Hierarchy.Extensions.System
{
	internal static class ObjectExtensions
	{
		private const BindingFlags _instanceBinding = (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
		private const BindingFlags _staticBinding = (BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

		internal static Boolean HasField<TObject>(this TObject systemObject, String fieldName)
		{
			return systemObject.GetType().HasField(fieldName);
		}

		internal static TObject GetInstanceProperty<TObject>(this Object systemObject, String propertyName)
		{
			if (systemObject == null)
			{
				throw new ArgumentNullException(nameof(systemObject));
			}

			return (TObject)systemObject.GetType().FindProperty(propertyName, _instanceBinding).GetValue(systemObject, null);
		}

		internal static void SetInstanceProperty<TObject, TValue>(this TObject systemObject, String propertyName, TValue value)
		{
			if (systemObject == null)
			{
				throw new ArgumentNullException(nameof(systemObject));
			}

			systemObject.GetType().FindProperty(propertyName, _instanceBinding).SetValue(systemObject, value, null);
		}

		internal static void InvokeMethod<TFirstArgument, TSecondArgument, TThirdArgument>(this Object systemObject, String methodName, TFirstArgument firstArgument, TSecondArgument secondArgument, TThirdArgument thirdArgument)
		{
			var arguments = new Object[]
			{
				firstArgument,
				secondArgument,
				thirdArgument,
			};

			var argumentsTypes = new Type[]
			{
				typeof(TFirstArgument),
				typeof(TSecondArgument),
				typeof(TThirdArgument),
			};

			RawCall(systemObject.GetType(), systemObject, methodName, arguments, argumentsTypes, false);
		}

		internal static TResult InvokeMethod<TResult, TArgument>(this Object systemObject, String methodName, TArgument argument)
		{
			var arguments = new Object[]
			{
				argument,
			};

			var argumentsTypes = new Type[]
			{
				typeof(TArgument),
			};

			return (TResult)RawCall(systemObject.GetType(), systemObject, methodName, arguments, argumentsTypes, false);
		}

		internal static Boolean HasMethod<TFirstArgument, TSecondArgument, TThirdArgument>(this Object systemObject, String methodName)
		{
			var argumentTypes = new Type[]
			{
				typeof(TFirstArgument),
				typeof(TSecondArgument),
				typeof(TThirdArgument),
			};

			return systemObject.GetType().FindMethod(methodName, argumentTypes) != null;
		}

		internal static TObject GetInstanceField<TObject>(this Object systemObject, String fieldName)
		{
			if (systemObject is null)
			{
				throw new ArgumentNullException(nameof(systemObject));
			}

			return (TObject)systemObject.GetType().FindField(fieldName, _instanceBinding).GetValue(systemObject);
		}

		internal static void SetInstanceField<TObject, TValue>(this TObject systemObject, String fieldName, TValue value)
		{
			if (systemObject == null)
			{
				throw new ArgumentNullException(nameof(systemObject));
			}

			systemObject.GetType().FindField(fieldName, _instanceBinding).SetValue(systemObject, value);
		}

		private static Object RawCall(Type type, Object systemObject, String methodName, Object[] arguments, Type[] argumentsTypes, Boolean isStatic)
		{
			if ((systemObject == null) && (!isStatic))
			{
				throw new ArgumentNullException(nameof(systemObject), "Object cannot be null for instance methods");
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
	}
}