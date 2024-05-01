using System;
using Editor.Hierarchy.GUIItems;
using UnityEngine;
using Object = System.Object;

namespace Editor.Hierarchy
{
	internal interface IPrefItem
	{
		internal Object Value { get; set; }

		internal GUIContent Label { get; }

		internal Boolean Drawing { get; }

		internal GUIBooleanFlag GetEnabledScope(Boolean enabled);

		internal GUIFade GetFadeScope(Boolean enabled);

		internal GUIBooleanFlag GetEnabledScope();
	}
}