using System.Linq;
using Editor.Hierarchy.Enums;
using Editor.Hierarchy.GUIItems;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	internal sealed class LockIcon : HierarchyIcon
	{
		internal override Texture2D PreferencesPreview => Utility.GetBackground(Styles.LockToggleStyle, false);

		protected override IconPosition Side => IconPosition.All;

		internal override void HandleGUIDraw(Rect rect)
		{
			var locked = (CustomHierarchy.CurrentGameObject.hideFlags & HideFlags.NotEditable) != 0;

			using (new GUIBackgroundColor(locked ? Styles.BackgroundColorEnabled : Styles.BackgroundColorDisabled))
			{
				GUI.changed = false;
				GUI.Toggle(rect, locked, Styles.LockContent, Styles.LockToggleStyle);

				if (!GUI.changed)
				{
					return;
				}

				var selectedObjects = GetSelectedObjectsAndCurrent();
				var changeMode = AskChangeModeIfNecessary(selectedObjects, Preferences.LockAskMode.Value, "Lock Object", "Do you want to " + (!locked ? "lock" : "unlock") + " the children objects as well?");

				switch (changeMode)
				{
					case ChildrenChangeMode.ObjectOnly:
					{
						foreach (var obj in selectedObjects)
						{
							Undo.RegisterCompleteObjectUndo(obj, locked ? "Unlock Object" : "Lock Object");
						}

						foreach (var selectedObject in selectedObjects)
						{
							if (!locked)
							{
								Utility.LockObject(selectedObject);
							}
							else
							{
								Utility.UnlockObject(selectedObject);
							}
						}

						break;
					}
					case ChildrenChangeMode.ObjectAndChildren:
					{
						foreach (var obj in selectedObjects)
						{
							Undo.RegisterFullObjectHierarchyUndo(obj, locked ? "Unlock Object" : "Lock Object");
						}

						foreach (var transform in selectedObjects.SelectMany(obj => obj.GetComponentsInChildren<Transform>(true)))
						{
							if (!locked)
							{
								Utility.LockObject(transform.gameObject);
							}
							else
							{
								Utility.UnlockObject(transform.gameObject);
							}
						}

						break;
					}
				}

				InternalEditorUtility.RepaintAllViews();
			}
		}
	}
}