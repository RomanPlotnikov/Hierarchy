using System;
using UnityEngine;

namespace Editor.Hierarchy.GUIItems
{
	internal readonly struct GUIColor : IDisposable
	{
		private readonly Color _previousColor;

		internal GUIColor(Color color, Single alpha)
		{
			_previousColor = GUI.color;

			color.a = alpha;
			GUI.color = color;
		}

		internal GUIColor(Color color)
		{
			_previousColor = GUI.color;

			GUI.color = color;
		}

		void IDisposable.Dispose()
		{
			GUI.color = _previousColor;
		}
	}
}