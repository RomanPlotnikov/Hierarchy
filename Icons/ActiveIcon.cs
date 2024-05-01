using Editor.Hierarchy.GUIItems;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class ActiveIcon : HierarchyIcon
	{
		internal override Texture2D PreferencesPreview => Utility.GetBackground(Styles.ActiveToggleStyle, true);

		protected override IconPosition Side => IconPosition.All;

		internal override void HandleGUIDraw(Rect rect)
		{
			using (new GUIBackgroundColor(CustomHierarchy.CurrentGameObject.activeSelf ? Styles.BackgroundColorEnabled : Styles.BackgroundColorDisabled))
			{
				GUI.changed = false;
				GUI.Toggle(rect, CustomHierarchy.CurrentGameObject.activeSelf, Styles.ActiveContent, Styles.ActiveToggleStyle);

				if (!GUI.changed)
				{
					return;
				}

				var gameObjects = GetSelectedObjectsAndCurrent();
				var currentGameObjectIsActive = !(CustomHierarchy.CurrentGameObject.activeSelf);

				var recordedObjects = new Object[gameObjects.Count];

				for (var gameObjectIndex = 0; (gameObjectIndex < gameObjects.Count); gameObjectIndex++)
				{
					recordedObjects[gameObjectIndex] = gameObjects[gameObjectIndex];
				}

				Undo.RecordObjects(recordedObjects, (CustomHierarchy.CurrentGameObject.activeSelf ? "Disabled GameObject" : "Enabled GameObject"));

				foreach (var gameObject in gameObjects)
				{
					gameObject.SetActive(currentGameObjectIsActive);
				}
			}
		}
	}
}