using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Editor.Hierarchy.Enums;
using Editor.Hierarchy.Extensions.System;
using Editor.Hierarchy.Icons;
using Editor.Hierarchy.MiniLabels;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Editor.Hierarchy
{
	internal static partial class Preferences
	{
		public static MiniLabelProvider[] MiniLabelProviders;

		static Preferences()
		{
			InitializePreferences();

			LeftSideButtonPref.DefaultValue = new IconData { HierarchyIcon = new EmptyIcon() };

			LineColor.DefaultValue = DefaultLineColor;
			OddRowColor.DefaultValue = DefaultOddSortColor;
			EvenRowColor.DefaultValue = DefaultEvenSortColor;
			HoverTintColor.DefaultValue = DefaultHoverTint;

			var defaultLeftIcons = new IconList
			{
				new WarningsIcon(),
				new SoundHierarchyIcon(),
			};

			var defaultRightIcons = new IconList
			{
				new ActiveIcon(),
				new LockIcon(),
				new StaticIcon(),
			};

			LeftIcons.DefaultValue = defaultLeftIcons;
			RightIcons.DefaultValue = defaultRightIcons;

			MiniLabels.DefaultValue = new[]
			{
				Array.IndexOf(MiniLabelProvider.MiniLabelsTypes, typeof(LayerMiniLabel)),
				Array.IndexOf(MiniLabelProvider.MiniLabelsTypes, typeof(TagMiniLabel))
			};

			_miniLabelsNames = MiniLabelProvider.MiniLabelsTypes.Select(ml => ml == null ? "None" : ObjectNames.NicifyVariableName(ml.Name.Replace("MiniLabel", ""))).ToArray();

			_leftIconsList = GenerateReorderableList(LeftIcons);
			_rightIconsList = GenerateReorderableList(RightIcons);

			_leftIconsList.onAddDropdownCallback = (rect, newList) => LeftIconsMenu.DropDown(rect);
			_rightIconsList.onAddDropdownCallback = (rect, newList) => RightIconsMenu.DropDown(rect);

			RecreateMiniLabelProviders();
		}

		private static Color DefaultOddSortColor => EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.1f) : new Color(1f, 1f, 1f, 0.2f);

		private static Color DefaultEvenSortColor => EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0f) : new Color(1f, 1f, 1f, 0f);

		private static Color DefaultLineColor => new Color(0f, 0f, 0f, 0.2f);

		private static Color DefaultHoverTint => EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.2f) : new Color(0.12f, 0.12f, 0.12f, 0.2f);

		public static HierarchyIcon LeftSideButton
		{
			get => LeftSideButtonPref.Value.HierarchyIcon;
			private set
			{
				LeftSideButtonPref.Value.HierarchyIcon = value;
				LeftSideButtonPref.ForceSave();
			}
		}

		private static Boolean MiniLabelTagEnabled => MiniLabelProviders.Any(ml => ml is TagMiniLabel);

		private static Boolean MiniLabelLayerEnabled => MiniLabelProviders.Any(ml => ml is LayerMiniLabel);

		public static Boolean EnhancedSelectionSupported => Application.platform == RuntimePlatform.WindowsEditor;

		private static void RecreateMiniLabelProviders()
		{
			MiniLabelProviders = MiniLabels.Value.Select(ml => MiniLabelProvider.MiniLabelsTypes.ElementAtOrDefault(ml)).Where(ml => ml != null).Select(ml => Activator.CreateInstance(ml) as MiniLabelProvider).ToArray();
		}

		public static Boolean IsButtonEnabled(HierarchyIcon button)
		{
			if (button == null)
			{
				return false;
			}

			if (LeftSideButton == button)
			{
				return true;
			}

			return RightIcons.Value.Contains(button) || LeftIcons.Value.Contains(button);
		}

		public static void ForceDisableButton(HierarchyIcon button)
		{
			if (button == null)
			{
				Debug.LogError("Removing null button");
			}
			else
			{
				Debug.LogWarning("Disabling \"" + button.Name + "\", most likely because it threw an exception");
			}

			if (LeftSideButton == button)
			{
				LeftSideButton = HierarchyIcon.EmptyIcon;
			}

			RightIcons.Value.Remove(button);
			LeftIcons.Value.Remove(button);

			RightIcons.ForceSave();
			LeftIcons.ForceSave();
		}

		private static void InitializePreferences()
		{
			var type = typeof(Preferences);
			var members = type.GetMembers(TypeExtensions.FullBinding);

			foreach (var member in members)
				try
				{
					if (member == null)
					{
						continue;
					}

					Type prefItemType;
					var prop = member as PropertyInfo;
					var field = member as FieldInfo;

					switch (member.MemberType)
					{
						case MemberTypes.Field:
							if (typeof(IPrefItem).IsAssignableFrom(field.FieldType))
							{
								prefItemType = field.FieldType;
							}
							else
							{
								continue;
							}

							break;

						case MemberTypes.Property:
							if (typeof(IPrefItem).IsAssignableFrom(prop.PropertyType))
							{
								prefItemType = prop.PropertyType;
							}
							else
							{
								continue;
							}

							break;

						default:
							continue;
					}

					var defaultValueAttribute = (AutoPrefItemDefaultValueAttribute)member.GetCustomAttributes(typeof(AutoPrefItemDefaultValueAttribute), true).FirstOrDefault();
					var labelAttribute = (AutoPrefItemLabelAttribute)member.GetCustomAttributes(typeof(AutoPrefItemLabelAttribute), true).FirstOrDefault();
					var keyAttribute = (AutoPrefItemAttribute)member.GetCustomAttributes(typeof(AutoPrefItemAttribute), true).FirstOrDefault();

					var key = member.Name;
					var defaultValue = (Object)null;
					var label = new GUIContent(key);

					if (keyAttribute == null)
					{
						continue;
					}

					if (!String.IsNullOrEmpty(keyAttribute.Key))
					{
						key = keyAttribute.Key;
					}

					if (labelAttribute != null)
					{
						label = labelAttribute.Label;
					}

					if (defaultValueAttribute != null)
					{
						defaultValue = defaultValueAttribute.DefaultValue;
					}

					var prefItem = Activator.CreateInstance(prefItemType, key, defaultValue, label.text, label.tooltip);

					switch (member.MemberType)
					{
						case MemberTypes.Field:
							field.SetValue(null, prefItem);
							break;

						case MemberTypes.Property:
							prop.SetValue(null, prefItem, null);
							break;
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
		}

		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
		private class AutoPrefItemAttribute : Attribute
		{
			internal String Key { get; }

			internal AutoPrefItemAttribute(String key = null)
			{
				Key = key;
			}
		}

		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
		private class AutoPrefItemDefaultValueAttribute : Attribute
		{
			internal Object DefaultValue { get; }

			internal AutoPrefItemDefaultValueAttribute(Object defaultValue)
			{
				DefaultValue = defaultValue;
			}
		}

		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
		private class AutoPrefItemLabelAttribute : Attribute
		{
			internal GUIContent Label { get; }

			internal AutoPrefItemLabelAttribute(String label, String tooltip = null)
			{
				Label = new GUIContent(label, tooltip);
			}
		}

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(0)]
		[AutoPrefItemLabel("Right Margin", "Margin for icons, useful if you have more extensions that also uses hierarchy")]
		public static PrefItem<Int32> RightMargin;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(0)]
		[AutoPrefItemLabel("Left Margin", "Margin for icons, useful if you have more extensions that also uses hierarchy")]
		public static PrefItem<Int32> LeftMargin;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(14)]
		[AutoPrefItemLabel("Indent", "Indent for labels, useful for thin hierarchies")]
		public static PrefItem<Int32> Indent;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(0.8f)]
		[AutoPrefItemLabel("Hierarchy Tree Opacity", "The opacity of the tree view lines connecting child transforms to their parent, useful if you have multiple children inside children")]
		public static PrefItem<Single> TreeOpacity;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(0.5f)]
		[AutoPrefItemLabel("Stem Proportion", "Stem length for hierarchy items that have no children")]
		public static PrefItem<Single> TreeStemProportion;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Select on Tree", "Select the parent when you click on the tree lines\n\nTHIS MAY AFFECT PERFORMANCE")]
		public static PrefItem<Boolean> SelectOnTree;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Tooltips", "Shows tooltips, like this one")]
		public static PrefItem<Boolean> Tooltips;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Relevant Tooltips Only", "Hide tooltips that have static texts")]
		public static PrefItem<Boolean> RelevantTooltipsOnly;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Enhanced selection", "Allow selecting GameObjects by dragging over them with right mouse button")]
		public static PrefItem<Boolean> EnhancedSelection;

		[AutoPrefItem]
		[AutoPrefItemLabel("Highlight tint", "Tint the item under the mouse cursor")]
		public static PrefItem<Color> HoverTintColor;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(false)]
		[AutoPrefItemLabel("Hide native icon", "Hide the native icon on the left side of the name, introducted in Unity 2018.3")]
		public static PrefItem<Boolean> DisableNativeIcon;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Trailing", "Append ... when names are bigger than the view area")]
		public static PrefItem<Boolean> Trailing;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Select locked objects", "Allow selecting objects that are locked")]
		public static PrefItem<Boolean> AllowSelectingLockedObjects;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(false)]
		[AutoPrefItemLabel("Pick locked objects", "Allow picking objects that are locked on scene view\nObjects locked before you change this option will not be affected\nRequires Unity 2019.3 or newer")]
		public static PrefItem<Boolean> AllowPickingLockedObjects;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Change all selected", "This will make the enable, lock, layer, tag and static buttons affect all selected objects in the hierarchy")]
		public static PrefItem<Boolean> ChangeAllSelected;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Left side button at leftmost", "Put the left button to the leftmost side of the hierarchy, if disabled it will be next to the game object name")]
		public static PrefItem<Boolean> LeftmostButton;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Open scripts of logs", "Clicking on warnings, logs or errors will open the script to edit in your IDE or text editor\n\nMAY AFFECT PERFORMANCE")]
		public static PrefItem<Boolean> OpenScriptsOfLogs;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(false)]
		[AutoPrefItemLabel("Replace default child toggle", "Replace the default toggle for expanding children to a new one that shows the children count")]
		public static PrefItem<Boolean> NumericChildExpand;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(15)]
		[AutoPrefItemLabel("Icons Size", "The size of the icons in pixels")]
		public static PrefItem<Int32> IconsSize;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Centralize when possible", "Centralize minilabel when there's only tag or only layer on it")]
		public static PrefItem<Boolean> CentralizeMiniLabelWhenPossible;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Hide \"Untagged\" tag", "Hide default tag on minilabel")]
		public static PrefItem<Boolean> HideDefaultTag;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(true)]
		[AutoPrefItemLabel("Hide \"Default\" layer", "Hide default layer on minilabel")]
		public static PrefItem<Boolean> HideDefaultLayer;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(false)]
		[AutoPrefItemLabel("Hide default icon", "Hide the default game object icon")]
		public static PrefItem<Boolean> HideDefaultIcon;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(1)]
		[AutoPrefItemLabel("Line thickness", "Separator line thickness")]
		public static PrefItem<Int32> LineSize;

		[AutoPrefItem]
		[AutoPrefItemLabel("Odd row tint", "The tint of odd rows")]
		public static PrefItem<Color> OddRowColor;

		[AutoPrefItem]
		[AutoPrefItemLabel("Even row tint", "The tint of even rows")]
		public static PrefItem<Color> EvenRowColor;

		[AutoPrefItem]
		[AutoPrefItemLabel("Line tint", "The tint of separators line")]
		public static PrefItem<Color> LineColor;

		[AutoPrefItem]
		[AutoPrefItemLabel("Left side button", "The button that will appear in the left side of the hierarchy\nLooks better with \"Hierarchy Tree\" disabled")]
		public static PrefItem<IconData> LeftSideButtonPref;

		[AutoPrefItem]
		[AutoPrefItemLabel("Mini label", "The little label next to the GameObject name")]
		public static PrefItem<Int32[]> MiniLabels;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(ChildrenChangeMode.ObjectAndChildren)]
		[AutoPrefItemLabel("Lock", "Which objects will be locked when you click on the lock toggle")]
		public static PrefItem<ChildrenChangeMode> LockAskMode;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(ChildrenChangeMode.Ask)]
		[AutoPrefItemLabel("Layer", "Which objects will have their layer changed when you click on the layer button or on the mini label")]
		public static PrefItem<ChildrenChangeMode> LayerAskMode;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(ChildrenChangeMode.ObjectOnly)]
		[AutoPrefItemLabel("Tag", "Which objects will have their tag changed when you click on the tag button or on the mini label")]
		public static PrefItem<ChildrenChangeMode> TagAskMode;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(ChildrenChangeMode.Ask)]
		[AutoPrefItemLabel("Static", "Which flags will be changed when you click on the static toggle")]
		public static PrefItem<ChildrenChangeMode> StaticAskMode;

		[AutoPrefItem]
		[AutoPrefItemDefaultValue(ChildrenChangeMode.ObjectOnly)]
		[AutoPrefItemLabel("Icon", "Which objects will have their icon changed when you click on the icon button")]
		public static PrefItem<ChildrenChangeMode> IconAskMode;

		[AutoPrefItem]
		[AutoPrefItemLabel("Icons next to the object name", "The icons that appear next to the game object name")]
		public static PrefItem<IconList> LeftIcons;

		[AutoPrefItem]
		[AutoPrefItemLabel("Icons on the rightmost", "The icons that appear to the rightmost of the hierarchy")]
		public static PrefItem<IconList> RightIcons;
	}
}