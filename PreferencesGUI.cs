using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Editor.Hierarchy.GUIItems;
using Editor.Hierarchy.Icons;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = System.Object;

namespace Editor.Hierarchy
{
	internal static partial class Preferences
	{
		public static readonly List<GUIContent> Contents = new List<GUIContent>();

		private static Vector2 _scroll;

		private static readonly ReorderableList _leftIconsList;
		private static readonly ReorderableList _rightIconsList;

		private static readonly String[] _miniLabelsNames;

		private static GenericMenu LeftIconsMenu => GetGenericMenuForIcons(LeftIcons, HierarchyIcon.AllLeftIcons);

		private static GenericMenu RightIconsMenu => GetGenericMenuForIcons(RightIcons, HierarchyIcon.AllRightIcons);

		private static GenericMenu GetGenericMenuForIcons<TList>(PrefItem<TList> preferenceItem, IEnumerable<HierarchyIcon> hierarchyIcons) where TList : IList
		{
			var genericMenu = new GenericMenu();

			foreach (var hierarchyIcon in hierarchyIcons)
			{
				if (!preferenceItem.Value.Contains(hierarchyIcon) && hierarchyIcon != HierarchyIcon.EmptyIcon && (hierarchyIcon != HierarchyIcon.EmptyIcon))
				{
					genericMenu.AddItem(new GUIContent(hierarchyIcon.Name), false, () =>
					{
						preferenceItem.Value.Add(hierarchyIcon);
						preferenceItem.ForceSave();
					});
				}
			}

			return genericMenu;
		}

		private static ReorderableList GenerateReorderableList<TList>(PrefItem<TList> preferenceItem) where TList : IList
		{
			var reorderableList = new ReorderableList(preferenceItem.Value, typeof(TList), true, true, true, true);

			reorderableList.elementHeight = 18.0F;

			reorderableList.drawHeaderCallback = rect =>
			{
				rect.xMin -= (EditorGUI.indentLevel * 16.0F);
				EditorGUI.LabelField(rect, preferenceItem, EditorStyles.boldLabel);
			};

			reorderableList.onChangedCallback += list => preferenceItem.ForceSave();

			reorderableList.drawElementCallback = (rect, index, _, _) =>
			{
				var icon = reorderableList.list[index] as HierarchyIcon;

				if (icon == null)
				{
					EditorGUI.LabelField(rect, "INVALID ICON");
					return;
				}

				var content = Utility.GetTempGUIContent(icon.Name, icon.PreferencesTooltip, icon.PreferencesPreview);
				var whiteTexture = ((!content.image) || content.image.name.Contains("eh_icon_white"));

				using (new GUIColor(whiteTexture && !EditorGUIUtility.isProSkin ? Styles.BackgroundColorEnabled : Color.white))
				{
					EditorGUI.LabelField(rect, content);
				}
			};

			return reorderableList;
		}

		[SettingsProvider]
		private static SettingsProvider RetrieveSettingsProvider()
		{
			var settingsProvider = new SettingsProvider("Preferences/Enhanced Hierarchy", SettingsScope.User, Contents.Select(c => c.text));

			settingsProvider.guiHandler = OnPreferencesGUI;

			return settingsProvider;
		}

		private static void OnPreferencesGUI(String search)
		{
			_scroll = EditorGUILayout.BeginScrollView(_scroll, false, false);

			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Each item has a tooltip explaining what it does, keep the mouse over it to see.", MessageType.Info);
			EditorGUILayout.Separator();

			using (new GUIIndent("Misc settings"))
			{
				using (new GUIIndent("Margins"))
				{
					RightMargin.DoGUISlider(-50, 50);

					using (new GUIBooleanFlag(Reflected.HierarchyArea.Supported))
					{
						LeftMargin.DoGUISlider(-50, 50);
						Indent.DoGUISlider(0, 35);
					}

					if (!Reflected.HierarchyArea.Supported)
					{
						EditorGUILayout.HelpBox("Custom Indent and Margins are not supported in this Unity version", MessageType.Warning);
					}
				}

				IconsSize.DoGUISlider(13, 23);
				TreeOpacity.DoGUISlider(0f, 1f);

				using (new GUIIndent())
				{
					using (((IPrefItem)SelectOnTree).GetFadeScope(TreeOpacity.Value > 0.01f))
					{
						SelectOnTree.DoGUI();
					}

					using (((IPrefItem)TreeStemProportion).GetFadeScope(TreeOpacity.Value > 0.01f))
					{
						TreeStemProportion.DoGUISlider(0f, 1f);
					}
				}

				Tooltips.DoGUI();

				using (new GUIIndent())
				using (((IPrefItem)RelevantTooltipsOnly).GetFadeScope(Tooltips))
				{
					RelevantTooltipsOnly.DoGUI();
				}

				if (EnhancedSelectionSupported)
				{
					EnhancedSelection.DoGUI();
				}

				Trailing.DoGUI();
				ChangeAllSelected.DoGUI();
				NumericChildExpand.DoGUI();

				using (new GUIBooleanFlag(Reflected.IconWidthSupported))
				{
					DisableNativeIcon.DoGUI();
				}

				using (((IPrefItem)HideDefaultIcon).GetFadeScope(IsButtonEnabled(new GameObjectHierarchyIcon())))
				{
					HideDefaultIcon.DoGUI();
				}

				using (((IPrefItem)OpenScriptsOfLogs).GetFadeScope(IsButtonEnabled(new WarningsIcon())))
				{
					OpenScriptsOfLogs.DoGUI();
				}

				GUI.changed = false;

				using (((IPrefItem)AllowSelectingLockedObjects).GetFadeScope(IsButtonEnabled(new LockIcon())))
				{
					AllowSelectingLockedObjects.DoGUI();
				}

				using (((IPrefItem)AllowPickingLockedObjects).GetFadeScope(IsButtonEnabled(new LockIcon())))
				{
					AllowPickingLockedObjects.DoGUI();
				}

				HoverTintColor.DoGUI();
			}

			GUI.changed = false;
			MiniLabels.Value[0] = EditorGUILayout.Popup("Mini Label Top", MiniLabels.Value[0], _miniLabelsNames);
			MiniLabels.Value[1] = EditorGUILayout.Popup("Mini Label Bottom", MiniLabels.Value[1], _miniLabelsNames);

			if (GUI.changed)
			{
				MiniLabels.ForceSave();
				RecreateMiniLabelProviders();
			}

			using (new GUIIndent())
			{
				using (((IPrefItem)HideDefaultTag).GetFadeScope(MiniLabelTagEnabled))
				{
					HideDefaultTag.DoGUI();
				}

				using (((IPrefItem)HideDefaultLayer).GetFadeScope(MiniLabelLayerEnabled))
				{
					HideDefaultLayer.DoGUI();
				}

				using (((IPrefItem)CentralizeMiniLabelWhenPossible).GetFadeScope(MiniLabelProviders.Length >= 2))
				{
					CentralizeMiniLabelWhenPossible.DoGUI();
				}
			}

			LeftSideButtonPref.DoGUI();

			using (new GUIIndent())
			using (((IPrefItem)LeftmostButton).GetFadeScope(LeftSideButton != HierarchyIcon.EmptyIcon))
			{
				LeftmostButton.DoGUI();
			}

			using (new GUIIndent("Children behaviour on change"))
			{
				using (((IPrefItem)LockAskMode).GetFadeScope(IsButtonEnabled(new LockIcon())))
				{
					LockAskMode.DoGUI();
				}

				using (((IPrefItem)LayerAskMode).GetFadeScope(IsButtonEnabled(new LayerIcon()) || MiniLabelLayerEnabled))
				{
					LayerAskMode.DoGUI();
				}

				using (((IPrefItem)TagAskMode).GetFadeScope(IsButtonEnabled(new TagIcon()) || MiniLabelTagEnabled))
				{
					TagAskMode.DoGUI();
				}

				using (((IPrefItem)StaticAskMode).GetFadeScope(IsButtonEnabled(new StaticIcon())))
				{
					StaticAskMode.DoGUI();
				}

				using (((IPrefItem)IconAskMode).GetFadeScope(IsButtonEnabled(new GameObjectHierarchyIcon())))
				{
					IconAskMode.DoGUI();
				}

				EditorGUILayout.HelpBox($"Pressing down {Utility.CtrlKey} while clicking on a button will make it temporary have the opposite children change mode", MessageType.Info);
			}

			_leftIconsList.displayAdd = LeftIconsMenu.GetItemCount() > 0;
			_leftIconsList.DoLayoutList();

			_rightIconsList.displayAdd = RightIconsMenu.GetItemCount() > 0;
			_rightIconsList.DoLayoutList();

			EditorGUILayout.HelpBox("Alt + Click on child expand toggle makes it expand all the grandchildren too", MessageType.Info);

			if (IsButtonEnabled(new MemoryIcon()))
			{
				EditorGUILayout.HelpBox("\"Memory Used\" may create garbage and consequently framerate stuttering, leave it disabled if maximum performance is important for your project", MessageType.Warning);
			}

			if (IsButtonEnabled(new LockIcon()))
			{
				EditorGUILayout.HelpBox("Remember to always unlock your game objects when removing or disabling this extension, as you won't be able to unlock without it and may lose scene data", MessageType.Warning);
			}

			GUI.enabled = true;
			EditorGUILayout.EndScrollView();

			using (new EditorGUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
			}

			EditorGUILayout.Separator();
			Styles.ReloadTooltips();
			EditorApplication.RepaintHierarchyWindow();
		}

		private static void DoGUISlider(this PrefItem<Int32> prefItem, Int32 min, Int32 max)
		{
			if (((IPrefItem)prefItem).Drawing)
			{
				prefItem.Value = EditorGUILayout.IntSlider(prefItem, prefItem, min, max);
			}
		}

		private static void DoGUISlider(this PrefItem<Single> prefItem, Single min, Single max)
		{
			if (((IPrefItem)prefItem).Drawing)
			{
				prefItem.Value = EditorGUILayout.Slider(prefItem, prefItem, min, max);
			}
		}

		private static void DoGUI(this PrefItem<Boolean> prefItem)
		{
			if (((IPrefItem)prefItem).Drawing)
			{
				prefItem.Value = EditorGUILayout.Toggle(prefItem, prefItem);
			}
		}

		private static void DoGUI(this PrefItem<Color> prefItem)
		{
			if (((IPrefItem)prefItem).Drawing)
			{
				prefItem.Value = EditorGUILayout.ColorField(prefItem, prefItem);
			}
		}

		private static void DoGUI<TConvertible>(this PrefItem<TConvertible> prefItem) where TConvertible : struct, IConvertible
		{
			if (((IPrefItem)prefItem).Drawing)
			{
				prefItem.Value = (TConvertible)(Object)EditorGUILayout.EnumPopup(prefItem, (Enum)(Object)prefItem.Value);
			}
		}

		private static void DoGUI(this PrefItem<IconData> prefItem)
		{
			if (!((IPrefItem)prefItem).Drawing)
			{
				return;
			}

			var icons = HierarchyIcon.AllLeftOfNameIcons;
			var index = Array.IndexOf(icons, prefItem.Value.HierarchyIcon);
			var labels = (from icon in icons select new GUIContent(icon)).ToArray();

			index = EditorGUILayout.Popup(prefItem, index, labels);

			if ((index < 0) || (index >= icons.Length))
			{
				return;
			}

			if (prefItem.Value.HierarchyIcon.Name == icons[index].Name)
			{
				return;
			}

			prefItem.Value.HierarchyIcon = icons[index];
			prefItem.ForceSave();
		}
	}
}