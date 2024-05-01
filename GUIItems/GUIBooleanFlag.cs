using System;
using UnityEngine;

namespace Editor.Hierarchy.GUIItems
{
	internal readonly struct GUIBooleanFlag : IDisposable
	{
		private readonly Boolean _previousBooleanFlag;

		internal GUIBooleanFlag(Boolean enabled)
		{
			_previousBooleanFlag = GUI.enabled;

			GUI.enabled = _previousBooleanFlag && enabled;
		}

		void IDisposable.Dispose()
		{
			GUI.enabled = _previousBooleanFlag;
		}
	}
}