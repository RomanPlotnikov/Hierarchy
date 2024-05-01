using Editor.Hierarchy.Enums;
using Editor.Hierarchy.GUIItems;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class StaticIcon : HierarchyIcon
	{
		protected override IconPosition Side => IconPosition.All;

		internal override Texture2D PreferencesPreview => Utility.GetBackground(Styles.StaticToggleStyle, false);

		internal override void HandleGUIDraw(Rect rect)
		{
			using (new GUIBackgroundColor(CustomHierarchy.CurrentGameObject.isStatic ? Styles.BackgroundColorEnabled : Styles.BackgroundColorDisabled))
			{
				GUI.changed = false;
				GUI.Toggle(rect, CustomHierarchy.CurrentGameObject.isStatic, Styles.StaticContent, Styles.StaticToggleStyle);

				if (!GUI.changed)
				{
					return;
				}

				var isStatic = !CustomHierarchy.CurrentGameObject.isStatic;
				var selectedObjects = GetSelectedObjectsAndCurrent();
				var changeMode = AskChangeModeIfNecessary(selectedObjects, Preferences.StaticAskMode.Value, "Change Static Flags", "Do you want to " + (!isStatic ? "enable" : "disable") + " the static flags for all child objects as well?");

				switch (changeMode)
				{
					case ChildrenChangeMode.ObjectOnly:
					{
						foreach (var obj in selectedObjects)
						{
							Undo.RegisterCompleteObjectUndo(obj, "Static Flags Changed");
							obj.isStatic = isStatic;
						}

						break;
					}
					case ChildrenChangeMode.ObjectAndChildren:
					{
						foreach (var obj in selectedObjects)
						{
							Undo.RegisterFullObjectHierarchyUndo(obj, "Static Flags Changed");

							var transforms = obj.GetComponentsInChildren<Transform>(true);
							foreach (var transform in transforms)
								transform.gameObject.isStatic = isStatic;
						}

						break;
					}
				}
			}
		}
	}
}