using System;
using System.Diagnostics;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Editor.Hierarchy
{
	public static class After
	{
		public static void Condition(Func<Boolean> condition, Action callback, Double timeoutMs = 0.0D)
		{
			var callbackFunction = new EditorApplication.CallbackFunction(() => { });
			var timeoutsAt = (EditorApplication.timeSinceStartup + (timeoutMs / 1000.0D));
			var stack = new StackFrame(1, true);

			var function = callbackFunction;

			callbackFunction = () =>
			{
				if (timeoutMs > 0.0D && EditorApplication.timeSinceStartup >= timeoutsAt)
				{
					EditorApplication.update -= function;
					Debug.LogErrorFormat("Condition timeout at {0}:{1}", stack.GetFileName(), stack.GetFileLineNumber());
					return;
				}

				if (condition())
				{
					EditorApplication.update -= function;
					callback();
				}
			};

			EditorApplication.update += callbackFunction;
		}
	}
}