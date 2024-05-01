using System;
using System.Linq;
using Editor.Hierarchy.Extensions.System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Hierarchy
{
	internal static class Reflected
	{
		private static Boolean _gameObjectStylesTypeLoaded;
		private static Type _gameObjectTreeViewStylesType;

		private static readonly Type _hierarchyWindowType = "UnityEditor.SceneHierarchyWindow".FindType();

		private static EditorWindow _hierarchyWindowInstance;

		internal static Boolean HierarchyFocused => EditorWindow.focusedWindow && EditorWindow.focusedWindow.GetType() == _hierarchyWindowType;

		internal static EditorWindow HierarchyWindowInstance
		{
			get
			{
				if (_hierarchyWindowInstance)
				{
					return _hierarchyWindowInstance;
				}

				var lastHierarchy = (EditorWindow)null;

				try
				{
					lastHierarchy = _hierarchyWindowType.GetStaticField<EditorWindow>("s_LastInteractedHierarchy");
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}

				return (lastHierarchy ? (_hierarchyWindowInstance = lastHierarchy) : _hierarchyWindowInstance = (EditorWindow)Resources.FindObjectsOfTypeAll(_hierarchyWindowType).FirstOrDefault());
			}
		}

		private static System.Object SceneHierarchyOrWindow => HierarchyWindowInstance.GetInstanceProperty<System.Object>("sceneHierarchy");

		private static System.Object TreeView => SceneHierarchyOrWindow.GetInstanceProperty<System.Object>("treeView");

		private static System.Object TreeViewGUI => TreeView.GetInstanceProperty<System.Object>("gui");

		public static Boolean IconWidthSupported => TreeView != null && TreeViewGUI != null && TreeViewGUI.HasField("k_IconWidth");

		public static Single IconWidth
		{
			get
			{
				if (!IconWidthSupported)
				{
					return 0;
				}

				return TreeViewGUI.GetInstanceField<Single>("k_IconWidth");
			}
			set => TreeViewGUI.SetInstanceField("k_IconWidth", value);
		}

		private static Type GameObjectTreeViewStylesType
		{
			get
			{
				if (_gameObjectStylesTypeLoaded)
				{
					return _gameObjectTreeViewStylesType;
				}

				_gameObjectStylesTypeLoaded = true;
				_gameObjectTreeViewStylesType = TreeViewGUI.GetType().GetNestedType("GameObjectStyles", TypeExtensions.FullBinding);

				return _gameObjectTreeViewStylesType;
			}
		}

		public static Boolean NativeHierarchyHoverTintSupported => (GameObjectTreeViewStylesType != null) && GameObjectTreeViewStylesType.HasField("hoveredBackgroundColor");

		public static Color NativeHierarchyHoverTint
		{
			set
			{
				if (!NativeHierarchyHoverTintSupported)
				{
					Debug.LogWarning("Native hover tint not supported!");
					return;
				}

				GameObjectTreeViewStylesType.SetStaticField("hoveredBackgroundColor", value);
			}
		}

		public static void ShowIconSelector(Object[] objects, Rect activatorRect, Boolean showLabelIcons)
		{
			try
			{
				var iconSelectorType = "UnityEditor.IconSelector".FindType();

				if (iconSelectorType.HasMethod<Object[], Rect, Boolean>("ShowAtPosition"))
				{
					if (!iconSelectorType.InvokeStaticMethod<Boolean, Object[], Rect, Boolean>("ShowAtPosition", objects, activatorRect, showLabelIcons))
					{
						Debug.LogWarning("Failed to open icon selector");
					}

					return;
				}

				var instance = ScriptableObject.CreateInstance(iconSelectorType);

				if (instance.HasMethod<Object[], Rect, Boolean>("Init"))
				{
					instance.InvokeMethod("Init", objects, activatorRect, showLabelIcons);
				}
				else
				{
					var affectedObjects = objects.FirstOrDefault();
					instance.InvokeMethod("Init", affectedObjects, activatorRect, showLabelIcons);

					After.Condition(() => !instance, () =>
					{
						var icon = GetObjectIcon(affectedObjects);

						foreach (var unityObject in objects)
						{
							SetObjectIcon(unityObject, icon);
						}
					});
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarning("Failed to open icon selector\n" + exception);
			}
		}

		private static void SetObjectIcon(Object systemObject, Texture2D texture)
		{
			typeof(EditorGUIUtility).InvokeStaticMethod("SetIconForObject", systemObject, texture);
			EditorUtility.SetDirty(systemObject);
		}

		public static Texture2D GetObjectIcon(Object systemObject)
		{
			return typeof(EditorGUIUtility).InvokeStaticMethod<Texture2D, Object>("GetIconForObject", systemObject);
		}

		public static Boolean GetTransformIsExpanded(GameObject gameObject)
		{
			try
			{
				var data = TreeView.GetInstanceProperty<System.Object>("data");
				var isExpanded = data.InvokeMethod<Boolean, Int32>("IsExpanded", gameObject.GetInstanceID());

				return isExpanded;
			}
			catch (Exception exception)
			{
				Preferences.NumericChildExpand.Value = false;

				Debug.LogException(exception);
				Debug.LogWarningFormat("Disabled \"{0}\" because it failed to get hierarchy info", Preferences.NumericChildExpand.Label.text);

				return false;
			}
		}

		public static void SetHierarchySelectionNeedSync()
		{
			try
			{
				if (HierarchyWindowInstance)
				{
					SceneHierarchyOrWindow.SetInstanceProperty("selectionSyncNeeded", true);
				}
			}
			catch (Exception exception)
			{
				Debug.LogWarningFormat("Enabling \"{0}\" because it caused an exception", Preferences.AllowSelectingLockedObjects.Label.text);
				Debug.LogException(exception);

				Preferences.AllowSelectingLockedObjects.Value = true;
			}
		}

		internal static class HierarchyArea
		{
			private static Single _defaultBaseIndent = Single.NaN;

			static HierarchyArea()
			{
				Debug.LogWarning("HierarchyArea not supported!");
			}

			internal static Boolean Supported
			{
				get
				{
					try
					{
						return HierarchyWindowInstance && (TreeView is not null) && (TreeViewGUI is not null);
					}
					catch
					{
						return false;
					}
				}
			}

			internal static Single IndentWidth
			{
				get => TreeViewGUI.GetInstanceField<Single>("k_IndentWidth");
				set => TreeViewGUI.SetInstanceField("k_IndentWidth", value);
			}

			internal static Single BaseIndent
			{
				get
				{
					var baseIndent = TreeViewGUI.GetInstanceField<Single>("k_BaseIndent");

					if (Single.IsNaN(_defaultBaseIndent))
					{
						_defaultBaseIndent = baseIndent;
					}

					return baseIndent;
				}
				set
				{
					if (Single.IsNaN(_defaultBaseIndent))
					{
						_defaultBaseIndent = BaseIndent;
					}

					TreeViewGUI.SetInstanceField("k_BaseIndent", _defaultBaseIndent + value);
				}
			}
		}
	}
}