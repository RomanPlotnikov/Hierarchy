using System;

namespace Editor.Hierarchy.Icons
{
	[Flags]
	public enum IconPosition
	{
		AfterObjectName = 1,
		BeforeObjectName = 2,
		RightMost = 4,
		SafeArea = (AfterObjectName | RightMost),
		All = (SafeArea | BeforeObjectName),
	}
}