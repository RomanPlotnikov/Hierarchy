using UnityEngine;

namespace Editor.Hierarchy.Extensions.UnityEngine
{
	internal static class GUIStyleExtensions
	{
		internal static void InitializeBackgrounds(this GUIStyle guiStyle, Texture2D activeIconTexture2D, Texture2D inactiveIconTexture2D)
		{
			guiStyle.onFocused.background = activeIconTexture2D;
			guiStyle.onActive.background = activeIconTexture2D;
			guiStyle.onNormal.background = activeIconTexture2D;
			guiStyle.onHover.background = activeIconTexture2D;

			guiStyle.focused.background = inactiveIconTexture2D;
			guiStyle.active.background = inactiveIconTexture2D;
			guiStyle.normal.background = inactiveIconTexture2D;
			guiStyle.hover.background = inactiveIconTexture2D;

			guiStyle.imagePosition = ImagePosition.ImageOnly;
		}
	}
}