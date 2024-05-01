using System;
using UnityEngine;

namespace Editor.Hierarchy.Icons
{
	public sealed class EmptyIcon : HierarchyIcon
	{
		protected override IconPosition Side => IconPosition.All;

		internal override Single Width => 0.0F;

		internal override String Name => "None";

		internal override void HandleGUIDraw(Rect rect)
		{
		}
	}
}