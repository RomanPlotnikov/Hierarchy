using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Hierarchy
{
	internal static partial class CustomHierarchy
	{
		internal static readonly List<Component> Components = new List<Component>(64);
		internal const String Untagged = "Untagged";

		internal const Int32 UnLayered = 0;

		private static readonly GUIContent _trailingContent = new GUIContent("...");
		private const Single _alphaThreshold = 0.01F;

		internal static GameObject CurrentGameObject { get; private set; }

		internal static Boolean IsRepaintEvent { get; private set; }

		internal static String GameObjectTag { get; private set; }

		internal static Boolean IsGameObject { get; private set; }

		internal static Color CurrentColor { get; private set; }

		internal static Boolean HasLayer { get; private set; }

		internal static Boolean HasTag { get; private set; }

		private static List<Object> DragSelection { get; set; }

		private static EventType LastEventType { get; set; }

		private static Single RightIconsWidth { get; set; }

		private static Boolean IsFirstVisible { get; set; }

		private static Vector2 SelectionStart { get; set; }

		private static Single LeftIconsWidth { get; set; }

		private static String GameObjectName { get; set; }

		private static GUIStyle CurrentStyle { get; set; }

		private static Rect LabelOnlyRect { get; set; }

		private static Rect SelectionRect { get; set; }

		private static Rect FullSizeRect { get; set; }

		private static Single LabelSize { get; set; }

		private static Rect FinalRect { get; set; }

		private static Rect RawRect { get; set; }

		private static void DrawItemInformation(Int32 instanceID, Rect rect)
		{
			try
			{
				CurrentGameObject = (EditorUtility.InstanceIDToObject(instanceID) as GameObject);

				IsGameObject = CurrentGameObject;
				IsRepaintEvent = (Event.current.type == EventType.Repaint);
				IsFirstVisible = (Event.current.type != LastEventType);
				LastEventType = Event.current.type;

				if (CurrentGameObject)
				{
					GameObjectName = CurrentGameObject.name;

					try
					{
						GameObjectTag = CurrentGameObject.tag;
					}
					catch
					{
						Debug.LogWarning("Invalid game object tag", CurrentGameObject);

						GameObjectTag = "Untagged";
					}

					LabelSize = EditorStyles.label.CalcSize(Utility.GetTempGUIContent(GameObjectName)).x;
					LabelSize += (Reflected.IconWidth + 5.0F);

					var labelOnlyRect = rect;
					labelOnlyRect.xMax = (labelOnlyRect.xMin + LabelSize);
					LabelOnlyRect = labelOnlyRect;
					HasTag = ((!CurrentGameObject.CompareTag(Untagged)) || (!Preferences.HideDefaultTag));
					HasLayer = ((CurrentGameObject.layer != UnLayered) || (!Preferences.HideDefaultLayer));
					CurrentStyle = Utility.GetHierarchyLabelStyle(CurrentGameObject);
					CurrentColor = CurrentStyle.normal.textColor;
					CurrentGameObject.GetComponents(Components);
				}

				if (IsFirstVisible)
				{
					FinalRect = RawRect;
				}

				RawRect = rect;
				rect.xMin = 0.0F;

				FullSizeRect = rect;
			}
			catch (Exception exception)
			{
				Utility.LogException(exception);
			}
		}
	}
}