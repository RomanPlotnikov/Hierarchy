using System;
using Editor.Hierarchy.GUIItems;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Editor.Hierarchy
{
	[Serializable]
	public sealed class PrefItem<TObject> : IPrefItem
	{
		private const String _keyPrefix = "Hierarchy";

		private readonly GUIFade _fade;
		private TObject _defaultValue;

		private String _key;
		private Wrapper _wrapper;

		public PrefItem(String key, TObject defaultValue, String text = "", String tooltip = "")
		{
			_key = _keyPrefix + key;
			_defaultValue = defaultValue;

			Label = new GUIContent(text, tooltip);
			_fade = new GUIFade();

			Preferences.Contents.Add(Label);

			if (UsingDefaultValue)
			{
				_wrapper.Value = Clone(defaultValue);
			}
			else
			{
				LoadValue();
			}
		}

		internal TObject DefaultValue
		{
			get => _defaultValue;
			set => SetDefaultValue(value);
		}

		internal TObject Value
		{
			get => _wrapper.Value;
			set => SetValue(value, false);
		}

		private Boolean UsingDefaultValue => !EditorPrefs.HasKey(_key);

		internal GUIContent Label { get; set; }

		GUIContent IPrefItem.Label => Label;

		Boolean IPrefItem.Drawing => _fade.Visible;

		Object IPrefItem.Value
		{
			get => Value;
			set => Value = (TObject)value;
		}

		GUIBooleanFlag IPrefItem.GetEnabledScope()
		{
			return ((IPrefItem)this).GetEnabledScope(Value.Equals(true));
		}

		GUIBooleanFlag IPrefItem.GetEnabledScope(Boolean enabled)
		{
			return new GUIBooleanFlag(enabled);
		}

		GUIFade IPrefItem.GetFadeScope(Boolean enabled)
		{
			_fade.SetTarget(enabled);
			return _fade;
		}

		internal void SetDefaultValue(TObject newDefault)
		{
			if (UsingDefaultValue)
			{
				_wrapper.Value = Clone(newDefault);
			}

			_defaultValue = newDefault;
		}

		private void LoadValue()
		{
			try
			{
				if (!EditorPrefs.HasKey(_key))
				{
					return;
				}

				var json = EditorPrefs.GetString(_key);

				_wrapper = JsonUtility.FromJson<Wrapper>(json);
			}
			catch (Exception exception)
			{
				Debug.LogWarningFormat("Failed to load preference item \"{0}\", using default value: {1}", _key, _defaultValue);
				Debug.LogException(exception);

				ResetValue();
			}
		}

		private void SetValue(TObject newValue, Boolean forceSave)
		{
			try
			{
				if (Value != null && Value.Equals(newValue) && !forceSave)
				{
					return;
				}

				_wrapper.Value = newValue;

				var json = JsonUtility.ToJson(_wrapper);

				EditorPrefs.SetString(_key, json);
			}
			catch (Exception exception)
			{
				Debug.LogWarningFormat("Failed to save {0}: {1}", _key, exception);
				Debug.LogException(exception);
			}
			finally
			{
				_wrapper.Value = newValue;
			}
		}

		private void ResetValue()
		{
			if (UsingDefaultValue)
			{
				return;
			}

			Debug.LogFormat("Deleted preference {0}", _key);

			_wrapper.Value = Clone(_defaultValue);
			EditorPrefs.DeleteKey(_key);
		}

		internal void ForceSave()
		{
			SetValue(_wrapper.Value, true);
		}

		private TObject Clone(TObject other)
		{
			if (typeof(TObject).IsValueType)
			{
				return other;
			}

			var wrapper = new Wrapper { Value = other };
			var json = JsonUtility.ToJson(wrapper);
			var clonedWrapper = JsonUtility.FromJson<Wrapper>(json);

			return clonedWrapper.Value;
		}

		public static implicit operator TObject(PrefItem<TObject> pb)
		{
			if (pb != null)
			{
				return pb.Value;
			}

			Debug.LogError("Cannot get the value of a null PrefItem");
			return default;
		}

		public static implicit operator GUIContent(PrefItem<TObject> pb)
		{
			if (pb != null)
			{
				return pb.Label;
			}

			Debug.LogError("Cannot get the content of a null PrefItem");

			return new GUIContent("Null PrefItem");
		}

		[Serializable]
		private struct Wrapper
		{
			[SerializeField]
			internal TObject Value;
		}
	}
}