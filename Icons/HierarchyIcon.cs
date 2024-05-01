using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Hierarchy.Enums;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Editor.Hierarchy.Icons
{
	public abstract class HierarchyIcon
	{
		private const Single _defaultWidth = 16.0F;

		internal static readonly EmptyIcon EmptyIcon = new EmptyIcon();

		private static readonly Dictionary<String, HierarchyIcon> _icons;

		static HierarchyIcon()
		{
			var baseType = typeof(HierarchyIcon);

			_icons = baseType.Assembly.GetTypes().Where(type => (type != baseType) && baseType.IsAssignableFrom(type)).Select(type => (HierarchyIcon)Activator.CreateInstance(type)).ToDictionary(type => type.Name);

			AllLeftOfNameIcons = _icons.Select(hierarchyIcon => hierarchyIcon.Value).Where(hierarchyIcon => (hierarchyIcon.Side & IconPosition.BeforeObjectName) != 0).ToArray();
			AllLeftIcons = _icons.Select(hierarchyIcon => hierarchyIcon.Value).Where(hierarchyIcon => (hierarchyIcon.Side & IconPosition.AfterObjectName) != 0).ToArray();
			AllRightIcons = _icons.Select(hierarchyIcon => hierarchyIcon.Value).Where(hierarchyIcon => (hierarchyIcon.Side & IconPosition.RightMost) != 0).ToArray();
		}

		internal virtual Texture2D PreferencesPreview => null;

		internal virtual String Name => GetType().Name;

		internal virtual Single Width => _defaultWidth;

		internal String PreferencesTooltip => null;

		internal static HierarchyIcon[] AllLeftOfNameIcons { get; private set; }

		internal static HierarchyIcon[] AllRightIcons { get; private set; }

		internal static HierarchyIcon[] AllLeftIcons { get; private set; }

		protected virtual IconPosition Side => IconPosition.SafeArea;

		internal virtual void Initialize()
		{
		}

		internal abstract void HandleGUIDraw(Rect rect);

		internal static ChildrenChangeMode AskChangeModeIfNecessary(IEnumerable<GameObject> gameObjects, ChildrenChangeMode reference, String title, String message)
		{
			var controlPressed = Event.current.control || Event.current.command;

			switch (reference)
			{
				case ChildrenChangeMode.ObjectOnly:
				{
					return controlPressed ? ChildrenChangeMode.ObjectAndChildren : ChildrenChangeMode.ObjectOnly;
				}
				case ChildrenChangeMode.ObjectAndChildren:
				{
					return controlPressed ? ChildrenChangeMode.ObjectOnly : ChildrenChangeMode.ObjectAndChildren;
				}
				default:
				{
					foreach (var unused in gameObjects.Where(gameObject => gameObject && gameObject.transform.childCount > 0))
					{
						try
						{
							return (ChildrenChangeMode)EditorUtility.DisplayDialogComplex(title, message, "Yes, change children", "No, this object only", "Cancel");
						}
						finally
						{
							if (EditorWindow.focusedWindow)
							{
								EditorWindow.focusedWindow.Focus();
							}
						}
					}

					return ChildrenChangeMode.ObjectOnly;
				}
			}
		}

		protected List<GameObject> GetSelectedObjectsAndCurrent()
		{
			if (!Preferences.ChangeAllSelected || Selection.gameObjects.Length < 2)
			{
				return CustomHierarchy.CurrentGameObject ? new List<GameObject> { CustomHierarchy.CurrentGameObject } : new List<GameObject>();
			}

			return Selection.gameObjects.Where(obj => !EditorUtility.IsPersistent(obj)).Union(CustomHierarchy.CurrentGameObject ? new[] { CustomHierarchy.CurrentGameObject } : Array.Empty<GameObject>()).Distinct().ToList();
		}

		public override String ToString()
		{
			return Name;
		}

		public override Int32 GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override Boolean Equals(Object systemObject)
		{
			return systemObject as HierarchyIcon == this;
		}

		public static implicit operator HierarchyIcon(String name)
		{
			try
			{
				return _icons[name];
			}
			catch
			{
				return EmptyIcon;
			}
		}

		public static implicit operator String(HierarchyIcon hierarchyIcon)
		{
			return hierarchyIcon.ToString();
		}

		public static Boolean operator ==(HierarchyIcon left, HierarchyIcon right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}

			if (ReferenceEquals(left, null))
			{
				return false;
			}

			if (ReferenceEquals(right, null))
			{
				return false;
			}

			return left.Name == right.Name;
		}

		public static Boolean operator !=(HierarchyIcon left, HierarchyIcon right)
		{
			return !(left == right);
		}
	}
}