using System;
using Editor.Hierarchy.GUIItems;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Editor.Hierarchy.Icons
{
	internal sealed class MemoryIcon : HierarchyIcon
	{
		private static readonly GUIContent _label = new GUIContent();

		private Single _width;

		internal override String Name => "Memory Used";

		internal override Single Width => (_width + 4.0F);

		internal override void Initialize()
		{
			_width = 0.0F;

			if (!CustomHierarchy.IsGameObject)
			{
				return;
			}

			if (Preferences.Tooltips && !Preferences.RelevantTooltipsOnly)
			{
				_label.tooltip = "Used Memory";
			}
			else
			{
				_label.tooltip = String.Empty;
			}

			var memory = Profiler.GetRuntimeMemorySizeLong(CustomHierarchy.CurrentGameObject);

			if (memory == 0)
			{
				return;
			}

			_label.text = EditorUtility.FormatBytes(memory);
			_width = Styles.ApplyPrefabStyle.CalcSize(_label).x;
		}

		internal override void HandleGUIDraw(Rect rect)
		{
			if (_width <= 0.0F)
			{
				return;
			}

			rect.xMin += 2.0F;
			rect.xMax -= 2.0F;

			using (new GUIColor(Styles.BackgroundColorEnabled))
			{
				EditorGUI.LabelField(rect, _label, Styles.ApplyPrefabStyle);
			}
		}
	}
}