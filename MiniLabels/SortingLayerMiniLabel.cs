using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Editor.Hierarchy.MiniLabels
{
	public class SortingLayerMiniLabel : MiniLabelProvider
	{
		private const String _defaultSortingLayer = "Default";
		private String _layerName;

		private Int32 _sortingOrder;

		protected override void FillContent(GUIContent content)
		{
			var particleSystem = CustomHierarchy.Components.FirstOrDefault(component => (component is ParticleSystemRenderer)) as ParticleSystemRenderer;
			var spriteRenderer = CustomHierarchy.Components.FirstOrDefault(component => (component is SpriteRenderer)) as SpriteRenderer;
			var sortingGroup = CustomHierarchy.Components.FirstOrDefault(component => (component is SortingGroup)) as SortingGroup;

			Type comp = null;
			var hasSortingLayer = true;

			if (sortingGroup)
			{
				_layerName = sortingGroup.sortingLayerName;
				_sortingOrder = sortingGroup.sortingOrder;
				comp = sortingGroup.GetType();
			}
			else if (spriteRenderer)
			{
				_layerName = spriteRenderer.sortingLayerName;
				_sortingOrder = spriteRenderer.sortingOrder;
				comp = spriteRenderer.GetType();
			}
			else if (particleSystem)
			{
				_layerName = particleSystem.sortingLayerName;
				_sortingOrder = particleSystem.sortingOrder;
				comp = typeof(ParticleSystem);
			}
			else
			{
				hasSortingLayer = false;
			}

			content.text = hasSortingLayer ? $"{_layerName}:{_sortingOrder}" : String.Empty;

			content.tooltip = comp != null && Preferences.Tooltips ? $"Sorting layer from {comp.Name}" : String.Empty;
		}

		protected override Boolean Faded()
		{
			return _layerName == _defaultSortingLayer && _sortingOrder == 0;
		}

		protected override void OnClick()
		{
		}
	}
}