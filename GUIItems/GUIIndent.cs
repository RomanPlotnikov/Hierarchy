using System;
using UnityEditor;

namespace Editor.Hierarchy.GUIItems
{
	internal sealed class GUIIndent : IDisposable
	{
		internal GUIIndent()
		{
			EditorGUI.indentLevel++;
		}

		internal GUIIndent(String label)
		{
			EditorGUILayout.LabelField(label);
			EditorGUI.indentLevel++;
		}

		void IDisposable.Dispose()
		{
			EditorGUI.indentLevel--;
			EditorGUILayout.Separator();
		}
	}
}