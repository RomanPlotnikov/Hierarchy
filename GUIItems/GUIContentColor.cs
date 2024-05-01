using System;
using UnityEngine;

namespace Editor.Hierarchy.GUIItems
{
	public readonly struct GUIContentColor : IDisposable
	{
		private readonly Color _previousColor;

		internal GUIContentColor(Color color)
		{
			_previousColor = GUI.contentColor;

			GUI.contentColor = color;
		}

		void IDisposable.Dispose()
		{
			GUI.contentColor = _previousColor;
		}
	}
}