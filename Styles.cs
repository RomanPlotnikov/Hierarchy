using System;
using Editor.Hierarchy.Enums;
using Editor.Hierarchy.Extensions.UnityEngine;
using UnityEditor;
using UnityEngine;

namespace Editor.Hierarchy
{
	internal static class Styles
	{
		private static readonly GUIContent _rendererContent;

		public static readonly GUIContent PrefabApplyContent;
		public static readonly GUIContent StaticContent;
		public static readonly GUIContent ActiveContent;
		public static readonly GUIContent LayerContent;
		public static readonly GUIContent LockContent;
		public static readonly GUIContent TagContent;

		public static readonly GUIStyle StaticToggleStyle;
		public static readonly GUIStyle ActiveToggleStyle;
		public static readonly GUIStyle ApplyPrefabStyle;
		public static readonly GUIStyle LockToggleStyle;
		public static readonly GUIStyle MiniLabelStyle;
		public static readonly GUIStyle NewToggleStyle;
		public static readonly GUIStyle LayerStyle;
		public static readonly GUIStyle IconButton;
		public static readonly GUIStyle TagStyle;

		public static readonly GUIStyle LabelPrefabBrokenDisabled;
		public static readonly GUIStyle LabelPrefabDisabled;
		public static readonly GUIStyle LabelPrefabBroken;
		public static readonly GUIStyle LabelDisabled;
		public static readonly GUIStyle LabelPrefab;
		public static readonly GUIStyle LabelNormal;

		public static readonly Texture2D TreeElbowTexture2D;
		public static readonly Texture2D TreeLineTexture2D;
		public static readonly Texture2D TreeTeeTexture2D;

		public static readonly Texture2D WarningIconTexture2D;
		public static readonly Texture2D ErrorIconTexture2D;
		public static readonly Texture2D InfoIconTexture2D;

		public static readonly Texture2D FadeTexture;

		public static readonly Color BackgroundColorDisabled;
		public static readonly Color BackgroundColorEnabled;
		public static readonly Color SelectedUnfocusedColor;
		public static readonly Color SelectedFocusedColor;
		public static readonly Color ChildToggleColor;
		public static readonly Color NormalColor;

		static Styles()
		{
			if (EditorGUIUtility.isProSkin)
			{
				BackgroundColorEnabled = new Color32(155, 155, 155, 255);
				BackgroundColorDisabled = new Color32(155, 155, 155, 100);
				NormalColor = new Color32(56, 56, 56, 255);
				SelectedFocusedColor = new Color32(62, 95, 150, 255);
				SelectedUnfocusedColor = new Color32(72, 72, 72, 255);
			}
			else
			{
				BackgroundColorEnabled = new Color32(65, 65, 65, 255);
				BackgroundColorDisabled = new Color32(65, 65, 65, 120);
				NormalColor = new Color32(194, 194, 194, 255);
				SelectedFocusedColor = new Color32(62, 125, 231, 255);
				SelectedUnfocusedColor = new Color32(143, 143, 143, 255);
			}

			ChildToggleColor = new Color32(30, 30, 30, 255);

			LockToggleStyle = CreateGUIStyle(IconType.LockOn, IconType.LockOff);

			ActiveToggleStyle = CreateGUIStyle(IconType.ActiveOn, IconType.ActiveOff);

			CreateGUIStyle(IconType.RenderOn, IconType.RenderOff);

			StaticToggleStyle = CreateGUIStyle(IconType.StaticOn, IconType.StaticOff);

			TagStyle = CreateGUIStyle(IconType.Tag, IconType.Tag);
			TagStyle.padding = new RectOffset(5, 17, 0, 1);
			TagStyle.border = new RectOffset();

			LayerStyle = CreateGUIStyle(IconType.Layers, IconType.Layers);
			LayerStyle.padding = new RectOffset(5, 17, 0, 1);
			LayerStyle.border = new RectOffset();

			TreeElbowTexture2D = EncodedIcons.CreateOrLoad(IconType.TreeElbow);
			TreeLineTexture2D = EncodedIcons.CreateOrLoad(IconType.TreeLine);
			TreeTeeTexture2D = EncodedIcons.CreateOrLoad(IconType.TreeTee);

			WarningIconTexture2D = EncodedIcons.CreateOrLoad(IconType.Warning);
			ErrorIconTexture2D = EncodedIcons.CreateOrLoad(IconType.Error);
			InfoIconTexture2D = EncodedIcons.CreateOrLoad(IconType.Info);

			FadeTexture = EncodedIcons.CreateOrLoad(IconType.Fade);

			LabelNormal = new GUIStyle("PR Label");
			LabelDisabled = new GUIStyle("PR DisabledLabel");
			LabelPrefab = new GUIStyle("PR PrefabLabel");
			LabelPrefabDisabled = new GUIStyle("PR DisabledPrefabLabel");
			LabelPrefabBroken = new GUIStyle("PR BrokenPrefabLabel");
			LabelPrefabBrokenDisabled = new GUIStyle("PR DisabledBrokenPrefabLabel");

			MiniLabelStyle = new GUIStyle("ShurikenLabel")
			{
				alignment = TextAnchor.MiddleRight,
				clipping = TextClipping.Overflow,
			};

			MiniLabelStyle.focused.textColor = Color.white;
			MiniLabelStyle.normal.textColor = Color.white;
			MiniLabelStyle.active.textColor = Color.white;
			MiniLabelStyle.hover.textColor = Color.white;

			ApplyPrefabStyle = new GUIStyle("ShurikenLabel")
			{
				alignment = TextAnchor.MiddleCenter,
				clipping = TextClipping.Overflow,
			};

			ApplyPrefabStyle.focused.textColor = Color.white;
			ApplyPrefabStyle.normal.textColor = Color.white;
			ApplyPrefabStyle.active.textColor = Color.white;
			ApplyPrefabStyle.hover.textColor = Color.white;

			EditorApplication.update += () => ApplyPrefabStyle.fontSize = (Preferences.IconsSize - 6);

			IconButton = new GUIStyle("iconButton");
			IconButton.padding = new RectOffset();
			IconButton.margin = new RectOffset();

			NewToggleStyle = CreateGUIStyle(IconType.ChildToggleOn, IconType.ChildToggleOff, "ShurikenDropdown");

			NewToggleStyle.fontSize = 8;
			NewToggleStyle.clipping = TextClipping.Overflow;
			NewToggleStyle.alignment = TextAnchor.MiddleLeft;
			NewToggleStyle.imagePosition = ImagePosition.TextOnly;
			NewToggleStyle.border = new RectOffset(0, 1, 0, 1);
			NewToggleStyle.contentOffset = new Vector2(1.0F, 0.0F);
			NewToggleStyle.padding = new RectOffset(0, 0, 0, 0);
			NewToggleStyle.overflow = new RectOffset(-1, 1, -3, 0);
			NewToggleStyle.fixedHeight = 0.0F;
			NewToggleStyle.fixedWidth = 0.0F;
			NewToggleStyle.stretchHeight = true;
			NewToggleStyle.stretchWidth = true;

			var textColor = new Color32(230, 230, 230, 255);

			NewToggleStyle.onFocused.textColor = textColor;
			NewToggleStyle.onActive.textColor = textColor;
			NewToggleStyle.onNormal.textColor = textColor;
			NewToggleStyle.focused.textColor = textColor;
			NewToggleStyle.onHover.textColor = textColor;
			NewToggleStyle.active.textColor = textColor;
			NewToggleStyle.normal.textColor = textColor;
			NewToggleStyle.hover.textColor = textColor;

			PrefabApplyContent = new GUIContent("A");
			StaticContent = new GUIContent();
			LockContent = new GUIContent();
			ActiveContent = new GUIContent();
			_rendererContent = new GUIContent();
			TagContent = new GUIContent();
			LayerContent = new GUIContent();

			ReloadTooltips();
		}

		public static void ReloadTooltips()
		{
			if (Preferences.Tooltips && (!Preferences.RelevantTooltipsOnly))
			{
				PrefabApplyContent.tooltip = "Apply Prefab Changes";
				_rendererContent.tooltip = "Enable/Disable renderer";
				ActiveContent.tooltip = "Enable/Disable";
				LockContent.tooltip = "Lock/Unlock";
				StaticContent.tooltip = "Static";
				LayerContent.tooltip = "Layer";
				TagContent.tooltip = "Tag";
			}
			else
			{
				PrefabApplyContent.tooltip = String.Empty;
				_rendererContent.tooltip = String.Empty;
				StaticContent.tooltip = String.Empty;
				ActiveContent.tooltip = String.Empty;
				LayerContent.tooltip = String.Empty;
				LockContent.tooltip = String.Empty;
				TagContent.tooltip = String.Empty;
			}
		}

		private static GUIStyle CreateGUIStyle(IconType activeIconType, IconType inactiveIconType, GUIStyle referenceGUIStyle = null)
		{
			var guiStyle = ((referenceGUIStyle is null) ? new GUIStyle() : new GUIStyle(referenceGUIStyle));

			guiStyle.InitializeBackgrounds(EncodedIcons.CreateOrLoad(activeIconType), EncodedIcons.CreateOrLoad(inactiveIconType));

			EditorApplication.update += () =>
			{
				guiStyle.fixedHeight = Preferences.IconsSize;
				guiStyle.fixedWidth = Preferences.IconsSize;
			};

			return guiStyle;
		}
	}
}