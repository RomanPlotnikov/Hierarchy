using System;
using Editor.Hierarchy.Icons;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.MiniLabels
{
	public class LayerMiniLabel : MiniLabelProvider
	{
		protected override void FillContent(GUIContent content)
		{
			content.text = CustomHierarchy.HasLayer ? LayerMask.LayerToName(CustomHierarchy.CurrentGameObject.layer) : String.Empty;
		}

		protected override Boolean Faded()
		{
			return CustomHierarchy.CurrentGameObject.layer == CustomHierarchy.UnLayered;
		}

		protected override Boolean Draw(Rect rect, GUIContent content, GUIStyle style)
		{
			GUI.changed = false;

			var layer = EditorGUI.LayerField(rect, CustomHierarchy.CurrentGameObject.layer, Styles.MiniLabelStyle);

			if (GUI.changed)
			{
				LayerIcon.ChangeLayerAndAskForChildren(CustomHierarchy.GetSelectedObjectsAndCurrent(), layer);
			}

			return GUI.changed;
		}

		protected override void OnClick()
		{
		}
	}
}