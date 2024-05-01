using System;
using System.Collections.Generic;
using Editor.Hierarchy.Enums;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class LayerIcon : HierarchyIcon
	{
		internal override Texture2D PreferencesPreview => Utility.GetBackground(Styles.LayerStyle, true);

		protected override IconPosition Side => IconPosition.All;

		internal override void HandleGUIDraw(Rect rect)
		{
			GUI.changed = false;

			EditorGUI.LabelField(rect, Styles.LayerContent);
			var layer = EditorGUI.LayerField(rect, CustomHierarchy.CurrentGameObject.layer, Styles.LayerStyle);

			if (GUI.changed)
			{
				ChangeLayerAndAskForChildren(GetSelectedObjectsAndCurrent(), layer);
			}
		}

		public static void ChangeLayerAndAskForChildren(List<GameObject> gameObjects, Int32 newLayer)
		{
			var changeMode = AskChangeModeIfNecessary(gameObjects, Preferences.LayerAskMode, "Change Layer",
				"Do you want to change the layers of the children objects as well?");

			switch (changeMode)
			{
				case ChildrenChangeMode.ObjectOnly:
				{
					foreach (var gameObject in gameObjects)
					{
						Undo.RegisterCompleteObjectUndo(gameObject, "Layer changed");

						gameObject.layer = newLayer;
					}

					break;
				}
				case ChildrenChangeMode.ObjectAndChildren:
				{
					foreach (var gameObject in gameObjects)
					{
						Undo.RegisterFullObjectHierarchyUndo(gameObject, "Layer changed");

						gameObject.layer = newLayer;

						foreach (var transform in gameObject.GetComponentsInChildren<Transform>(true))
						{
							transform.gameObject.layer = newLayer;
						}
					}

					break;
				}
			}
		}
	}
}