using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor.Hierarchy.Icons
{
	[Serializable]
	public sealed class IconList : List<HierarchyIcon>, ISerializationCallbackReceiver
	{
		[FormerlySerializedAs("data")] [SerializeField]
		private IconData[] _iconsData;

		public IconList()
		{
		}

		public IconList(IEnumerable<HierarchyIcon> collection) : base(collection)
		{
		}

		public void OnAfterDeserialize()
		{
			if (_iconsData == null)
			{
				return;
			}

			Clear();

			foreach (var iconData in _iconsData)
			{
				Add(iconData.HierarchyIcon);
			}
		}

		public void OnBeforeSerialize()
		{
			_iconsData = new IconData[Count];

			for (var i = 0; i < _iconsData.Length; i++)
			{
				_iconsData[i] = new IconData
				{
					HierarchyIcon = this[i],
				};
			}
		}
	}
}