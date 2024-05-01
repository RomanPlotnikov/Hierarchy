using System;
using Editor.Hierarchy.GUIItems;
using UnityEngine;

namespace Editor.Hierarchy.MiniLabels
{
	public abstract class MiniLabelProvider
	{
		public static readonly Type[] MiniLabelsTypes =
		{
			null,
			typeof(TagMiniLabel),
			typeof(LayerMiniLabel),
			typeof(SortingLayerMiniLabel),
		};

		private readonly GUIContent _content = new GUIContent();

		protected abstract void FillContent(GUIContent content);

		protected abstract Boolean Faded();

		protected abstract void OnClick();

		public void Init()
		{
			FillContent(_content);
		}

		public Boolean HasValue()
		{
			return _content.text.Length > 0;
		}

		protected virtual Boolean Draw(Rect rect, GUIContent content, GUIStyle style)
		{
			return GUI.Button(rect, content, style);
		}

		public Single Measure()
		{
			var calculated = Styles.MiniLabelStyle.CalcSize(_content);
			return calculated.x;
		}

		public void Draw(ref Rect rect)
		{
			if (!HasValue())
			{
				return;
			}

			var color = CustomHierarchy.CurrentColor;
			var alpha = Faded() ? Styles.BackgroundColorDisabled.a : Styles.BackgroundColorEnabled.a;
			var finalColor = color * new Color(1f, 1f, 1f, alpha);

			using (new GUIContentColor(finalColor))
			{
				Styles.MiniLabelStyle.fontSize = 8;
				rect.xMin -= Measure();

				if (Draw(rect, _content, Styles.MiniLabelStyle))
				{
					OnClick();
				}
			}
		}
	}
}