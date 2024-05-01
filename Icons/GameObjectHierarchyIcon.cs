using System;
using System.Linq;
using Editor.Hierarchy.Enums;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	internal sealed class GameObjectHierarchyIcon : HierarchyIcon
	{
		private GUIContent _lastContent;

		internal override String Name => "GameObject Icon";

		protected override IconPosition Side => IconPosition.All;

		internal override Single Width => _lastContent.image ? base.Width : 0f;

		internal override Texture2D PreferencesPreview => AssetPreview.GetMiniTypeThumbnail(typeof(GameObject));

		internal override void Initialize()
		{
			_lastContent ??= new GUIContent();

			_lastContent.text = String.Empty;

			_lastContent.image = Preferences.HideDefaultIcon ? Reflected.GetObjectIcon(CustomHierarchy.CurrentGameObject) : AssetPreview.GetMiniThumbnail(CustomHierarchy.CurrentGameObject);

			_lastContent.tooltip = ((Preferences.Tooltips && (!Preferences.RelevantTooltipsOnly)) ? "Change Icon" : String.Empty);
		}

		internal override void HandleGUIDraw(Rect rect)
		{
			rect.yMin++;
			rect.xMin++;

			GUI.changed = false;
			GUI.Button(rect, _lastContent, EditorStyles.label);

			if (!GUI.changed)
			{
				return;
			}

			var affectedObjsList = GetSelectedObjectsAndCurrent();
			var gameObjects = affectedObjsList.AsEnumerable();
			var changeMode = AskChangeModeIfNecessary(affectedObjsList, Preferences.IconAskMode.Value, "Change Icons", "Do you want to change children icons as well?");

			gameObjects = changeMode switch
			{
				ChildrenChangeMode.ObjectAndChildren => gameObjects.SelectMany(gameObject => gameObject.GetComponentsInChildren<Transform>(true).Select(transform => transform.gameObject)),
				_ => gameObjects,
			};

			gameObjects = gameObjects.Distinct();

			var affectedObjsArray = gameObjects.ToArray();

			foreach (var obj in affectedObjsArray)
				Undo.RegisterCompleteObjectUndo(obj, "Icon Changed");

			Reflected.ShowIconSelector(affectedObjsArray, rect, true);
		}
	}
}