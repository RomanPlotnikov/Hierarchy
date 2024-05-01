using System;
using UnityEditor;
using UnityEditor.AnimatedValues;

namespace Editor.Hierarchy.GUIItems
{
	internal sealed class GUIFade : IDisposable
	{
		private AnimBool _animationBoolean;

		internal Boolean Visible { get; private set; } = true;

		void IDisposable.Dispose()
		{
			EditorGUILayout.EndFadeGroup();
		}

		internal void SetTarget(Boolean target)
		{
			if (_animationBoolean is null)
			{
				_animationBoolean = new AnimBool(target);

				_animationBoolean.valueChanged.AddListener(RepaintWindow);
			}

			_animationBoolean.target = target;

			Visible = EditorGUILayout.BeginFadeGroup(_animationBoolean.faded);

			return;

			void RepaintWindow()
			{
				if (EditorWindow.focusedWindow)
				{
					EditorWindow.focusedWindow.Repaint();
				}
			}
		}
	}
}