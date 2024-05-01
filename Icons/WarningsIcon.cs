using System;
using System.Linq;
using System.Text;
using Editor.Hierarchy.Enums;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class WarningsIcon : HierarchyIcon
	{
		private static readonly StringBuilder _goLogs = new StringBuilder(_maxStringLen);
		private static readonly StringBuilder _goWarnings = new StringBuilder(_maxStringLen);
		private static readonly StringBuilder _goErrors = new StringBuilder(_maxStringLen);
		private static readonly GUIContent _tempTooltipContent = new GUIContent();

		private LogEntry _warning;
		private LogEntry _error;
		private LogEntry _log;

		private const Int32 _maxStringLen = 750;
		private const Single _iconsWidth = 16.0F;

		internal override String Name => "Logs, Warnings and Errors";

		internal override Single Width
		{
			get
			{
				var result = 0.0F;

				if (_goLogs.Length > 0)
				{
					result += _iconsWidth;
				}

				if (_goWarnings.Length > 0)
				{
					result += _iconsWidth;
				}

				if (_goErrors.Length > 0)
				{
					result += _iconsWidth;
				}

				return result;
			}
		}

		internal override Texture2D PreferencesPreview => Styles.WarningIconTexture2D;

		internal override void Initialize()
		{
			if (!CustomHierarchy.IsGameObject)
			{
				return;
			}

			_warning = null;
			_error = null;
			_log = null;

			_goWarnings.Length = 0;
			_goErrors.Length = 0;
			_goLogs.Length = 0;

			var components = CustomHierarchy.Components;

			foreach (var unused in components.Where(component => !component))
			{
				_goWarnings.AppendLine("Missing MonoBehaviour\n");
			}

			foreach (var entry in LogEntry.CompileEntries.Where(entry => entry.ClassType != null).Where(entry => CustomHierarchy.Components.Any(comp => comp && (comp.GetType() == entry.ClassType || comp.GetType().IsAssignableFrom(entry.ClassType)))))
			{
				var isWarning = entry.HasMode(EntryMode.ScriptCompileWarning | EntryMode.AssetImportWarning);

				if ((_goWarnings.Length < _maxStringLen) && isWarning)
				{
					_goWarnings.AppendLine(entry.ToString());
				}

				else if ((_goErrors.Length < _maxStringLen) && (!isWarning))
				{
					_goErrors.AppendLine(entry.ToString());
				}

				if (isWarning && (_warning == null) && (!String.IsNullOrEmpty(entry.File)))
				{
					_warning = entry;
				}

				if (!isWarning && (_error == null) && (!String.IsNullOrEmpty(entry.File)))
				{
					_error = entry;
				}
			}

			if (LogEntry.GameObjectEntries.TryGetValue(CustomHierarchy.CurrentGameObject, out var contextEntries))
			{
				foreach (var entry in contextEntries)
				{
					var isLog = entry.HasMode(EntryMode.ScriptingLog);
					var isWarning = entry.HasMode(EntryMode.ScriptingWarning);
					var isError = entry.HasMode(EntryMode.ScriptingError | EntryMode.ScriptingException | EntryMode.ScriptingAssertion);

					if (isLog && _goLogs.Length < _maxStringLen)
					{
						_goLogs.AppendLine(entry.ToString());
					}

					else if (isWarning && (_goWarnings.Length < _maxStringLen))
					{
						_goWarnings.AppendLine(entry.ToString());
					}

					else if (isError && (_goErrors.Length < _maxStringLen))
					{
						_goErrors.AppendLine(entry.ToString());
					}

					if (isLog && (_log == null) && (!String.IsNullOrEmpty(entry.File)))
					{
						_log = entry;
					}

					if (isWarning && (_warning == null) && (!String.IsNullOrEmpty(entry.File)))
					{
						_warning = entry;
					}

					if (isError && (_error == null) && (!String.IsNullOrEmpty(entry.File)))
					{
						_error = entry;
					}
				}
			}
		}

		internal override void HandleGUIDraw(Rect rect)
		{
			if ((!CustomHierarchy.IsRepaintEvent && !Preferences.OpenScriptsOfLogs) || !CustomHierarchy.IsGameObject)
			{
				return;
			}

			rect.xMax = (rect.xMin + 17.0F);
			rect.yMax += 1.0F;

			DoSingleGUI(ref rect, _goLogs, Styles.InfoIconTexture2D, _log);
			DoSingleGUI(ref rect, _goWarnings, Styles.WarningIconTexture2D, _warning);
			DoSingleGUI(ref rect, _goErrors, Styles.ErrorIconTexture2D, _error);
		}

		private void DoSingleGUI(ref Rect rect, StringBuilder str, Texture texture, LogEntry logEntry)
		{
			if (str.Length == 0)
			{
				return;
			}

			if (Utility.ShouldCalculateTooltipAt(rect))
			{
				_tempTooltipContent.tooltip = Preferences.Tooltips ? str.ToString().TrimEnd('\n', '\r') : String.Empty;
			}

			_tempTooltipContent.image = texture;

			if (GUI.Button(rect, _tempTooltipContent, Styles.IconButton))
			{
				logEntry?.OpenToEdit();
			}

			rect.x += _iconsWidth;
		}
	}
}