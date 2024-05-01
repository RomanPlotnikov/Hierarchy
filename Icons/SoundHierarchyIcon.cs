using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class SoundHierarchyIcon : HierarchyIcon
	{
		private static AudioSource _audioSource;
		private static AnimBool _currentAnim;
		private static readonly Dictionary<AudioSource, AnimBool> _sourcesAnim = new Dictionary<AudioSource, AnimBool>();
		private static Texture _icon;

		static SoundHierarchyIcon()
		{
			EditorApplication.update += () =>
			{
				if (!Preferences.IsButtonEnabled(new SoundHierarchyIcon()))
				{
					return;
				}

				foreach (var (audioSource, animBool) in _sourcesAnim.Where(keyValuePair => (keyValuePair.Key && (keyValuePair.Value != null))))
				{
					animBool.target = audioSource.isPlaying;
				}
			};
		}

		internal override String Name => "Audio Source Icon";

		internal override Single Width
		{
			get
			{
				if (!_audioSource || _currentAnim == null)
				{
					return 0f;
				}

				return _currentAnim.faded * (base.Width - 2f);
			}
		}

		internal override Texture2D PreferencesPreview => AssetPreview.GetMiniTypeThumbnail(typeof(AudioSource));

		internal override void Initialize()
		{
			if (!CustomHierarchy.IsGameObject)
			{
				return;
			}

			_audioSource = null;

			foreach (var component in CustomHierarchy.Components)
			{
				if (component is AudioSource audioSource)
				{
					_audioSource = audioSource;
					break;
				}
			}

			if (!_audioSource)
			{
				return;
			}

			if (!_sourcesAnim.TryGetValue(_audioSource, out _currentAnim))
			{
				_sourcesAnim[_audioSource] = _currentAnim = new AnimBool(_audioSource.isPlaying);
				_currentAnim.valueChanged.AddListener(EditorApplication.RepaintHierarchyWindow);
			}
		}

		internal override void HandleGUIDraw(Rect rect)
		{
			if ((!CustomHierarchy.IsRepaintEvent) || (!CustomHierarchy.IsGameObject) || (!_audioSource) || (Width <= 1.0F))
			{
				return;
			}

			if (!_icon)
			{
				_icon = EditorGUIUtility.ObjectContent(null, typeof(AudioSource)).image;
			}

			rect.yMax -= 1.0F;
			rect.yMin += 1.0F;

			GUI.DrawTexture(rect, _icon, ScaleMode.StretchToFill);
		}
	}
}