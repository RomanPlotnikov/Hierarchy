using System;
using System.Text;
using Editor.Hierarchy.Icons;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Editor.Hierarchy
{
	internal static class Utility
	{
		private const String _ctrl = "Ctrl";
		private const String _cmd = "Cmd";

		private static Int32 _errorCount;

		private static readonly GUIContent _tempContent = new GUIContent();

		public static String CtrlKey => Application.platform == RuntimePlatform.OSXEditor ? _cmd : _ctrl;

		public static Boolean ShouldCalculateTooltipAt(Rect area)
		{
			return area.Contains(Event.current.mousePosition);
		}

		public static void LogException(Exception exception)
		{
			Debug.LogError("Unexpected exception in Enhanced Hierarchy");
			Debug.LogException(exception);
		}

		public static Texture2D GetBackground(GUIStyle guiStyle, Boolean isTurnedOn)
		{
			return isTurnedOn ? guiStyle.onNormal.background : guiStyle.normal.background;
		}

		public static Color GetHierarchyColor(Transform transform)
		{
			return ((!transform) ? Color.clear : GetHierarchyColor(transform.gameObject));
		}

		private static Color GetHierarchyColor(GameObject gameObject)
		{
			return ((!gameObject) ? Color.black : GetHierarchyLabelStyle(gameObject).normal.textColor);
		}

		public static GUIStyle GetHierarchyLabelStyle(GameObject gameObject)
		{
			if (!gameObject)
			{
				return EditorStyles.label;
			}

			var active = gameObject.activeInHierarchy;

			var prefabType = PrefabUtility.GetPrefabInstanceStatus(gameObject);

			return prefabType switch
			{
				PrefabInstanceStatus.MissingAsset => (active ? Styles.LabelPrefabBroken : Styles.LabelPrefabBrokenDisabled),
				PrefabInstanceStatus.Connected => (active ? Styles.LabelPrefab : Styles.LabelPrefabDisabled),
				_ => (active ? Styles.LabelNormal : Styles.LabelDisabled),
			};
		}

		public static Boolean TransformIsLastChild(Transform transform)
		{
			if (!transform)
			{
				return true;
			}

			return (transform.GetSiblingIndex() == (transform.parent.childCount - 1));
		}

		private static void ApplyHideFlagsToPrefab(UnityEngine.Object unityObject)
		{
			var handle = PrefabUtility.GetPrefabInstanceHandle(unityObject);

			if (handle)
			{
				handle.hideFlags = unityObject.hideFlags;
			}
		}

		public static void LockObject(GameObject gameObject)
		{
			gameObject.hideFlags |= HideFlags.NotEditable;
			ApplyHideFlagsToPrefab(gameObject);

			if (!Preferences.AllowPickingLockedObjects)
			{
				SceneVisibilityManager.instance.DisablePicking(gameObject, false);
			}

			EditorUtility.SetDirty(gameObject);
		}

		public static void UnlockObject(GameObject gameObject)
		{
			gameObject.hideFlags &= ~HideFlags.NotEditable;
			ApplyHideFlagsToPrefab(gameObject);

			if (!Preferences.AllowPickingLockedObjects)
			{
				SceneVisibilityManager.instance.EnablePicking(gameObject, false);
			}

			EditorUtility.SetDirty(gameObject);
		}

		public static void ApplyPrefabModifications(GameObject gameObject, Boolean allowCreatingNew)
		{
			var isPrefab = PrefabUtility.IsPartOfAnyPrefab(gameObject);

			if (isPrefab)
			{
				var prefab = PrefabUtility.GetNearestPrefabInstanceRoot(gameObject);

				if (!prefab)
				{
					Debug.LogError("Prefab asset not valid!");
					return;
				}

				if (PrefabUtility.GetPrefabInstanceStatus(prefab) == PrefabInstanceStatus.Connected)
				{
					PrefabUtility.ApplyPrefabInstance(prefab, InteractionMode.UserAction);
				}
				else if (EditorUtility.DisplayDialog("Apply disconnected prefab", "This is a disconnected game object, do you want to try to reconnect to the last prefab asset?", "Try to Reconnect", "Cancel"))
				{
					PrefabUtility.RevertPrefabInstance(prefab, InteractionMode.UserAction);
				}

				EditorUtility.SetDirty(prefab);
			}
			else if (allowCreatingNew)
			{
				var path = EditorUtility.SaveFilePanelInProject("Save prefab", "New Prefab", "prefab", "Save the selected prefab");

				if (!String.IsNullOrEmpty(path))
				{
					PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.UserAction);
				}
			}
		}

		public static String EnumFlagsToString(Enum value)
		{
			try
			{
				if ((Int32)(Object)value == -1)
				{
					return "Everything";
				}

				var stringBuilder = new StringBuilder();
				const String separator = ", ";

				foreach (var enumValue in Enum.GetValues(value.GetType()))
				{
					var enumIndex = (Int32)enumValue;

					if (enumIndex != 0 && (enumIndex & (enumIndex - 1)) == 0 && Enum.IsDefined(value.GetType(), enumIndex) && (Convert.ToInt32(value) & enumIndex) != 0)
					{
						stringBuilder.Append(ObjectNames.NicifyVariableName(enumValue.ToString()));
						stringBuilder.Append(separator);
					}
				}

				if (stringBuilder.Length > 0)
				{
					stringBuilder.Length -= separator.Length;
				}

				return stringBuilder.ToString();
			}
			catch (Exception exception)
			{
				Debug.LogError($"[{typeof(Utility)}] {exception}");
				return String.Empty;
			}
		}

		public static GUIContent GetTempGUIContent(String text, String tooltip = null, Texture2D image = null)
		{
			_tempContent.tooltip = tooltip;
			_tempContent.image = image;
			_tempContent.text = text;

			return _tempContent;
		}

		public static Single SafeGetWidth(this HierarchyIcon hierarchyIcon)
		{
			try
			{
				return (hierarchyIcon.Width + ((Preferences.IconsSize - 15) / 2.0F));
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Preferences.ForceDisableButton(hierarchyIcon);

				return 0.0F;
			}
		}

		public static void SafeInit(this HierarchyIcon hierarchyIcon)
		{
			try
			{
				hierarchyIcon.Initialize();
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Preferences.ForceDisableButton(hierarchyIcon);
			}
		}

		public static void SafeDoGUI(this HierarchyIcon hierarchyIcon, Rect rect)
		{
			try
			{
				rect.yMin -= ((Preferences.IconsSize - 15.0F) / 2.0F);
				rect.xMin -= ((Preferences.IconsSize - 15.0F) / 2.0F);

				hierarchyIcon.HandleGUIDraw(rect);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				Preferences.ForceDisableButton(hierarchyIcon);
			}
		}

		public static Rect FlipRectHorizontally(Rect rect)
		{
			return Rect.MinMaxRect(rect.xMax, rect.yMin, rect.xMin, rect.yMax);
		}
	}
}