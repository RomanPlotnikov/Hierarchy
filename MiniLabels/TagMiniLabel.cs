using System;
using Editor.Hierarchy.Icons;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.MiniLabels
{
	public class TagMiniLabel : MiniLabelProvider
	{
		protected override void FillContent(GUIContent content)
		{
			content.text = CustomHierarchy.HasTag ? CustomHierarchy.GameObjectTag : String.Empty;
		}

		protected override Boolean Faded()
		{
			return CustomHierarchy.GameObjectTag == CustomHierarchy.Untagged;
		}

		protected override Boolean Draw(Rect rect, GUIContent content, GUIStyle style)
		{
			GUI.changed = false;

			var tag = EditorGUI.TagField(rect, CustomHierarchy.GameObjectTag, style);

			if (GUI.changed)
			{
				TagIcon.ChangeTagAndAskForChildren(CustomHierarchy.GetSelectedObjectsAndCurrent(), tag);
			}

			return GUI.changed;
		}

		protected override void OnClick()
		{
		}
	}
}