using System;
using System.Collections.Generic;
using Editor.Hierarchy.Enums;
using Editor.Hierarchy.Extensions.System;
using Editor.Hierarchy.Icons;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Hierarchy
{
	public sealed class LogEntry
	{
		private const Double _updateSecondsCooldown = 0.75D;

		private static readonly Type _logEntriesType;
		private static readonly Type _logEntryType;

		public static readonly Dictionary<GameObject, List<LogEntry>> GameObjectEntries = new Dictionary<GameObject, List<LogEntry>>(100);
		public static readonly List<LogEntry> CompileEntries = new List<LogEntry>(100);
		private static readonly WarningsIcon _warningsIcon = new WarningsIcon();

		private static Boolean _lastCompileFailedState;
		private static Double _lastUpdatedTime;
		private static Boolean _entriesDirty;
		private static Int32 _lastCount;

		internal String File { get; private set; }

		internal Type ClassType { get; }

		private Object ObjectReference { get; }

		private MonoScript Script { get; }

		private Int32 InstanceID { get; }

		private String Condition { get; }

		private Int32 RowIndex { get; }

		private EntryMode Mode { get; }

		static LogEntry()
		{
			try
			{
				_logEntriesType = "UnityEditorInternal.LogEntries".FindType();
				_logEntryType = "UnityEditorInternal.LogEntry".FindType();

				_logEntriesType ??= "UnityEditor.LogEntries".FindType();
				_logEntryType ??= "UnityEditor.LogEntry".FindType();

				ReloadReferences();
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				Preferences.ForceDisableButton(new WarningsIcon());
			}

			Application.logMessageReceived += (_, _, _) => MarkEntriesDirty();

			EditorApplication.update += () =>
			{
				try
				{
					if ((!_entriesDirty) && (EditorUtility.scriptCompilationFailed != _lastCompileFailedState))
					{
						_lastCompileFailedState = EditorUtility.scriptCompilationFailed;
						MarkEntriesDirty();
					}

					if ((EditorApplication.timeSinceStartup - _lastUpdatedTime) > _updateSecondsCooldown)
					{
						if (!_entriesDirty)
						{
							var currentCount = GetLogCount();

							if (_lastCount > currentCount)
							{
								MarkEntriesDirty();
							}

							_lastCount = currentCount;
						}

						if (_entriesDirty)
						{
							ReloadReferences();
						}
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);

					Preferences.ForceDisableButton(new WarningsIcon());
				}
			};
		}

		private LogEntry(System.Object nativeEntry, Int32 rowIndex)
		{
			RowIndex = rowIndex;

			if (nativeEntry.HasField("condition"))
			{
				Condition = nativeEntry.GetInstanceField<String>("condition");
			}
			else if (nativeEntry.HasField("message"))
			{
				Condition = nativeEntry.GetInstanceField<String>("message");
			}
			else
			{
				throw new MissingFieldException("LogEntry doesn't have a message field");
			}

			if (nativeEntry.HasField("errorNum"))
			{
				nativeEntry.GetInstanceField<Int32>("errorNum");
			}

			File = nativeEntry.GetInstanceField<String>("file");
			nativeEntry.GetInstanceField<Int32>("line");

			if (nativeEntry.HasField("column"))
			{
				nativeEntry.GetInstanceField<Int32>("column");
			}

			Mode = nativeEntry.GetInstanceField<EntryMode>("mode");
			InstanceID = nativeEntry.GetInstanceField<Int32>("instanceID");
			nativeEntry.GetInstanceField<Int32>("identifier");

			if (InstanceID != 0)
			{
				ObjectReference = EditorUtility.InstanceIDToObject(InstanceID);
			}

			if (ObjectReference)
			{
				Script = ObjectReference as MonoScript;
			}

			if (Script)
			{
				ClassType = Script.GetClass();
			}
		}

		private static void MarkEntriesDirty()
		{
			if (!_entriesDirty && Preferences.IsButtonEnabled(_warningsIcon))
			{
				_entriesDirty = true;
			}
		}

		private static void ReloadReferences()
		{
			GameObjectEntries.Clear();
			CompileEntries.Clear();

			try
			{
				var count = _logEntriesType.InvokeStaticMethod<Int32>("StartGettingEntries");
				var nativeEntry = Activator.CreateInstance(_logEntryType);

				for (var i = 0; i < count; i++)
				{
					_logEntriesType.InvokeStaticMethod("GetEntryInternal", i, nativeEntry);

					var proxyEntry = new LogEntry(nativeEntry, i);
					var go = proxyEntry.ObjectReference as GameObject;

					if (proxyEntry.ObjectReference && !go)
					{
						var component = proxyEntry.ObjectReference as Component;

						if (component)
						{
							go = component.gameObject;
						}
					}

					if (proxyEntry.ClassType != null)
					{
						CompileEntries.Add(proxyEntry);
					}

					if (go)
					{
						if (GameObjectEntries.TryGetValue(go, out var entry))
						{
							entry.Add(proxyEntry);
						}
						else
						{
							GameObjectEntries.Add(go, new List<LogEntry> { proxyEntry });
						}
					}
				}

				EditorApplication.RepaintHierarchyWindow();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Preferences.ForceDisableButton(new WarningsIcon());
			}
			finally
			{
				_entriesDirty = false;
				_lastUpdatedTime = EditorApplication.timeSinceStartup;
				_logEntriesType.InvokeStaticMethod("EndGettingEntries");
			}
		}

		public Boolean HasMode(EntryMode toCheck)
		{
			return (Mode & toCheck) != 0;
		}

		public void OpenToEdit()
		{
			_logEntriesType.InvokeStaticMethod("RowGotDoubleClicked", RowIndex);
		}

		private static Int32 GetLogCount()
		{
			return _logEntriesType.InvokeStaticMethod<Int32>("GetCount");
		}

		public override String ToString()
		{
			return Condition;
		}
	}
}