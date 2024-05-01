using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Editor.Hierarchy.Extensions.System
{
	internal static class StringExtensions
	{
		private static Dictionary<String, Type> _cachedTypes;
		private static Assembly[] _cachedAssemblies;

		internal static Type FindType(this String typeName)
		{
			_cachedTypes ??= new Dictionary<String, Type>();

			if (_cachedTypes.TryGetValue(typeName, out var result))
			{
				return result;
			}

			result = FindTypeInAssembly(typeName, typeof(UnityEditor.Editor).Assembly);

			if (result == null)
			{
				_cachedAssemblies ??= AppDomain.CurrentDomain.GetAssemblies();

				foreach (var cachedAssembly in _cachedAssemblies)
				{
					result = FindTypeInAssembly(typeName, cachedAssembly);

					if (result != null)
					{
						break;
					}
				}
			}

			_cachedTypes[typeName] = result;
			return result;
		}

		private static Type FindTypeInAssembly(this String typeName, Assembly assembly)
		{
			return ((assembly == null) ? null : assembly.GetType(typeName, false, true));
		}
	}
}