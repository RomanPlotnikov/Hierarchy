using System;
using UnityEngine;

namespace Editor.Hierarchy.GUIItems
{
	internal readonly struct GUIBackgroundColor : IDisposable
	{
		private readonly Color _previousBackgroundColor;

		internal GUIBackgroundColor(Color color)
		{
			_previousBackgroundColor = GUI.backgroundColor;

			GUI.backgroundColor = color;
		}

		void IDisposable.Dispose()
		{
			GUI.backgroundColor = _previousBackgroundColor;
		}
	}
}