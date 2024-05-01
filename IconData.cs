using System;
using Editor.Hierarchy.Icons;
using UnityEngine;

namespace Editor.Hierarchy
{
	[Serializable]
	internal class IconData : ISerializationCallbackReceiver
	{
		[SerializeField]
		private String _name;

		internal HierarchyIcon HierarchyIcon { get; set; }

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			HierarchyIcon = _name;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			if (HierarchyIcon == null)
			{
				return;
			}

			_name = HierarchyIcon.Name;
		}
	}
}