using System;
using System.Collections.Generic;
using Editor.Hierarchy.Enums;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class TagIcon : HierarchyIcon
	{
		protected override IconPosition Side => IconPosition.All;

		internal override Texture2D PreferencesPreview => Utility.GetBackground(Styles.TagStyle, true);

		internal override void HandleGUIDraw(Rect rect)
		{
			GUI.changed = false;

			EditorGUI.LabelField(rect, Styles.TagContent);
			var tag = EditorGUI.TagField(rect, Styles.TagContent, CustomHierarchy.GameObjectTag, Styles.TagStyle);

			if (GUI.changed && tag != CustomHierarchy.GameObjectTag)
			{
				ChangeTagAndAskForChildren(GetSelectedObjectsAndCurrent(), tag);
			}
		}

		internal static void ChangeTagAndAskForChildren(List<GameObject> gameObjects, String newTag)
		{
			var changeMode = AskChangeModeIfNecessary(gameObjects, Preferences.TagAskMode, "Change Layer", "Do you want to change the tags of the children objects as well?");

			switch (changeMode)
			{
				case ChildrenChangeMode.ObjectOnly:
				{
					foreach (var obj in gameObjects)
					{
						Undo.RegisterCompleteObjectUndo(obj, "Tag changed");
						obj.tag = newTag;
					}

					break;
				}
				case ChildrenChangeMode.ObjectAndChildren:
				{
					foreach (var obj in gameObjects)
					{
						Undo.RegisterFullObjectHierarchyUndo(obj, "Tag changed");

						obj.tag = newTag;
						foreach (var transform in obj.GetComponentsInChildren<Transform>(true))
							transform.tag = newTag;
					}

					break;
				}
			}
		}
	}
}