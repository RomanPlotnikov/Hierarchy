using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Editor.Hierarchy.GUIItems;
using Editor.Hierarchy.MiniLabels;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Hierarchy
{
	[InitializeOnLoad]
	internal static partial class CustomHierarchy
	{
		static CustomHierarchy()
		{
			EditorApplication.hierarchyWindowItemOnGUI -= DrawItemInformation;
			EditorApplication.hierarchyWindowItemOnGUI -= OnItemGUI;

			EditorApplication.hierarchyWindowItemOnGUI += DrawItemInformation;
			EditorApplication.hierarchyWindowItemOnGUI += OnItemGUI;

			EditorApplication.RepaintHierarchyWindow();
		}

		private static MiniLabelProvider[] MiniLabelProviders => Preferences.MiniLabelProviders;

		private static void OnItemGUI(Int32 id, Rect rect)
		{
			try
			{
				if (IsGameObject)
				{
					foreach (var rightIcon in Preferences.RightIcons.Value)
					{
						rightIcon.SafeInit();
					}

					foreach (var leftIcon in Preferences.LeftIcons.Value)
					{
						leftIcon.SafeInit();
					}

					Preferences.LeftSideButton.SafeInit();

					foreach (var miniLabelProvider in MiniLabelProviders)
					{
						miniLabelProvider.Init();
					}
				}

				if (IsFirstVisible && Reflected.HierarchyArea.Supported)
				{
					Reflected.HierarchyArea.IndentWidth = Preferences.Indent;
					Reflected.HierarchyArea.BaseIndent = Preferences.LeftMargin;
				}

				CalculateIconsWidth();
				DoSelection(RawRect);
				IgnoreLockedSelection();
				DrawTree(RawRect);
				ChildToggle();
				var trailingWidth = DoTrailing();
				DrawHover();
				ColorSort(RawRect);
				DrawLeftSideIcons(RawRect);
				DrawTooltip(RawRect, trailingWidth);

				if (Reflected.IconWidthSupported)
				{
					Reflected.IconWidth = (Preferences.DisableNativeIcon ? 0.0F : 16.0F);
				}

				if (IsGameObject)
				{
					rect.xMax -= Preferences.RightMargin;
					rect.xMin = rect.xMax;
					rect.y++;

					foreach (var rightIcon in Preferences.RightIcons.Value)
					{
						using (new GUIBackgroundColor(Styles.BackgroundColorEnabled))
						{
							rect.xMin -= rightIcon.SafeGetWidth();
							rightIcon.SafeDoGUI(rect);
							rect.xMax -= rightIcon.SafeGetWidth();
						}
					}

					var leftSideRect = RawRect;

					if (Preferences.LeftmostButton)
					{
						leftSideRect.xMin = 0f;
					}
					else
					{
						leftSideRect.xMin -= (((2.0F + CurrentGameObject.transform.childCount > 0) || (Preferences.TreeOpacity > _alphaThreshold)) ? 30.0F : 18.0F);
					}

					leftSideRect.xMax = leftSideRect.xMin + Preferences.LeftSideButton.SafeGetWidth();

					using (new GUIBackgroundColor(Styles.BackgroundColorEnabled))
					{
						Preferences.LeftSideButton.SafeDoGUI(leftSideRect);
					}
				}

				DrawMiniLabel(ref rect);
				DrawHorizontalSeparator(RawRect);
			}
			catch (Exception exception)
			{
				Utility.LogException(exception);
			}
		}

		private static void DrawHover()
		{
			if (Reflected.NativeHierarchyHoverTintSupported)
			{
				if (IsFirstVisible && IsRepaintEvent)
				{
					Reflected.NativeHierarchyHoverTint = Preferences.HoverTintColor;
				}

				return;
			}

			var tint = Preferences.HoverTintColor.Value;

			if (IsFirstVisible && Reflected.NativeHierarchyHoverTintSupported)
			{
				Reflected.HierarchyWindowInstance.wantsMouseMove = (tint.a >= _alphaThreshold);
			}

			if (tint.a < _alphaThreshold)
			{
				return;
			}

			if (!Utility.ShouldCalculateTooltipAt(FullSizeRect))
			{
				return;
			}

			if (IsRepaintEvent)
			{
				EditorGUI.DrawRect(FullSizeRect, tint);
			}

			switch (Event.current.type)
			{
				case EventType.MouseMove:
					Event.current.Use();
					break;
			}
		}

		private static void IgnoreLockedSelection()
		{
			if (Preferences.AllowSelectingLockedObjects || (!IsFirstVisible) || (!IsRepaintEvent))
			{
				return;
			}

			var selection = Selection.objects;
			var changed = false;

			for (var i = 0; i < selection.Length; i++)
			{
				if (selection[i] is GameObject && (selection[i].hideFlags & HideFlags.NotEditable) != 0 && !EditorUtility.IsPersistent(selection[i]))
				{
					selection[i] = null;
					changed = true;
				}
			}

			if (changed)
			{
				Selection.objects = selection;
				Reflected.SetHierarchySelectionNeedSync();
				EditorApplication.RepaintHierarchyWindow();
			}
		}

		private static void ChildToggle()
		{
			if (!Preferences.NumericChildExpand || !IsRepaintEvent || !IsGameObject || CurrentGameObject.transform.childCount <= 0)
			{
				return;
			}

			var rect = RawRect;
			var childString = CurrentGameObject.transform.childCount.ToString("00");
			var expanded = Reflected.GetTransformIsExpanded(CurrentGameObject);

			rect.xMax = rect.xMin - 1f;
			rect.xMin -= 15f;

			if (childString.Length > 2)
			{
				rect.xMin -= 4f;
			}

			using (new GUIBackgroundColor(Styles.ChildToggleColor))
			{
				Styles.NewToggleStyle.Draw(rect, Utility.GetTempGUIContent(childString), false, false, expanded, false);
			}
		}

		private static void DrawHorizontalSeparator(Rect rect)
		{
			if (Preferences.LineSize < 1 || Preferences.LineColor.Value.a <= _alphaThreshold || !IsRepaintEvent)
			{
				return;
			}

			rect.xMin = 0.0F;
			rect.xMax += 50.0F;
			rect.yMin -= (Preferences.LineSize * 0.5F);
			rect.yMax = rect.yMin + Preferences.LineSize;

			EditorGUI.DrawRect(rect, Preferences.LineColor);

			if (!IsFirstVisible)
			{
				return;
			}

			rect.y = FinalRect.y - Preferences.LineSize * 0.5F;

			var height = Reflected.HierarchyWindowInstance.position.height;
			var count = (height - FinalRect.y) / FinalRect.height;

			if (FinalRect.height <= 0.0F)
			{
				count = 100.0F;
			}

			for (var i = 0; i < count; i++)
			{
				rect.y += RawRect.height;
				EditorGUI.DrawRect(rect, Preferences.LineColor);
			}
		}

		private static void ColorSort(Rect rect)
		{
			if (!IsRepaintEvent)
			{
				return;
			}

			rect.xMin = 0.0F;
			rect.xMax += 50.0F;

			var rowTint = GetRowTint();

			if (rowTint.a > _alphaThreshold)
			{
				EditorGUI.DrawRect(rect, rowTint);
			}

			if (!IsFirstVisible)
			{
				return;
			}

			rect.y = FinalRect.y;

			var height = Reflected.HierarchyWindowInstance.position.height;
			var count = (height - FinalRect.y) / FinalRect.height;

			if (FinalRect.height <= 0.0F)
			{
				count = 100.0F;
			}

			for (var i = 0; i < count; i++)
			{
				rect.y += RawRect.height;
				rowTint = GetRowTint(rect);

				if (rowTint.a > _alphaThreshold)
				{
					EditorGUI.DrawRect(rect, rowTint);
				}
			}
		}

		private static void DrawTree(Rect rect)
		{
			if (Preferences.TreeOpacity <= _alphaThreshold || !IsGameObject)
			{
				return;
			}

			if (!IsRepaintEvent && !Preferences.SelectOnTree)
			{
				return;
			}

			using (new GUIColor(Utility.GetHierarchyColor(CurrentGameObject.transform.parent), Preferences.TreeOpacity))
			{
				var indent = Reflected.HierarchyArea.Supported ? Reflected.HierarchyArea.IndentWidth : 16f;

				rect.x -= indent + 2f;

				rect.xMin -= 14f;
				rect.xMax = rect.xMin + 14f;

				if (CurrentGameObject.transform.parent)
				{
					var lastInHierarchy = Utility.TransformIsLastChild(CurrentGameObject.transform);

					GUI.DrawTexture(rect, lastInHierarchy ? Styles.TreeElbowTexture2D : Styles.TreeTeeTexture2D);

					var extendStemProportion = CurrentGameObject.transform.childCount == 0 ? Preferences.TreeStemProportion.Value * indent : indent - 14f;

					if (extendStemProportion > 0.01f)
					{
						var extendedStemRect = new Rect(rect.x + rect.size.x, rect.y + (lastInHierarchy ? 9f : 8f), extendStemProportion, 1f);
						EditorGUI.DrawRect(extendedStemRect, Color.white);
					}

					if (Preferences.SelectOnTree && GUI.Button(rect, GUIContent.none, Styles.LabelNormal))
					{
						Selection.activeTransform = CurrentGameObject.transform.parent;
					}
				}

				var currentTransform = CurrentGameObject.transform.parent;

				for (rect.x -= indent; rect.xMin > 0f && currentTransform && currentTransform.parent; rect.x -= indent)
				{
					if (!Utility.TransformIsLastChild(currentTransform))
					{
						using (new GUIColor(Utility.GetHierarchyColor(currentTransform.parent), Preferences.TreeOpacity))
						{
							GUI.DrawTexture(rect, Styles.TreeLineTexture2D);

							if (Preferences.SelectOnTree && GUI.Button(rect, GUIContent.none, Styles.LabelNormal))
							{
								Selection.activeTransform = currentTransform.parent;
							}
						}
					}

					currentTransform = currentTransform.parent;
				}
			}
		}

		private static void CalculateIconsWidth()
		{
			LeftIconsWidth = 0f;
			RightIconsWidth = 0f;

			if (!IsGameObject)
			{
				return;
			}

			foreach (var value in Preferences.RightIcons.Value)
			{
				RightIconsWidth += value.SafeGetWidth();
			}

			foreach (var icon in Preferences.LeftIcons.Value)
			{
				LeftIconsWidth += icon.SafeGetWidth();
			}
		}

		private static void DrawLeftSideIcons(Rect rect)
		{
			if (!IsGameObject)
			{
				return;
			}

			rect.xMin += LabelSize;
			rect.xMin = Math.Min(rect.xMax - RightIconsWidth - LeftIconsWidth - CalcMiniLabelSize() - 5f - Preferences.RightMargin, rect.xMin);

			foreach (var value in Preferences.LeftIcons.Value)
			{
				using (new GUIBackgroundColor(Styles.BackgroundColorEnabled))
				{
					rect.xMax = (rect.xMin + value.SafeGetWidth());
					value.SafeDoGUI(rect);
					rect.xMin = rect.xMax;
				}
			}
		}

		private static Single DoTrailing()
		{
			if (!IsRepaintEvent || !Preferences.Trailing || !IsGameObject)
			{
				return RawRect.xMax;
			}

			var iconsWidth = RightIconsWidth + LeftIconsWidth + CalcMiniLabelSize() + Preferences.RightMargin;

			var iconsMin = FullSizeRect.xMax - iconsWidth;
			var labelMax = LabelOnlyRect.xMax;

			var overlapping = iconsMin <= labelMax;

			if (!overlapping)
			{
				return labelMax;
			}

			var rect = FullSizeRect;

			rect.xMin = iconsMin - 18;
			rect.xMax = labelMax;

			if (Selection.gameObjects.Contains(CurrentGameObject))
			{
				EditorGUI.DrawRect(rect, Reflected.HierarchyFocused ? Styles.SelectedFocusedColor : Styles.SelectedUnfocusedColor);
			}
			else
			{
				EditorGUI.DrawRect(rect, Styles.NormalColor);
			}

			rect.y++;

			using (new GUIColor(CurrentColor))
			{
				EditorStyles.boldLabel.Draw(rect, _trailingContent, 0);
			}

			return iconsMin;
		}

		private static void DrawMiniLabel(ref Rect rect)
		{
			if (!IsGameObject)
			{
				return;
			}

			rect.x -= 3.0F;

			switch (MiniLabelProviders.Length)
			{
				case 0:
					return;

				case 1:
					if (MiniLabelProviders[0].HasValue())
					{
						MiniLabelProviders[0].Draw(ref rect);
					}

					break;

				default:
					var ml0 = MiniLabelProviders[0];
					var ml1 = MiniLabelProviders[1];
					var ml0HasValue = ml0.HasValue();
					var ml1HasValue = ml1.HasValue();

					if ((ml0HasValue && ml1HasValue) || !Preferences.CentralizeMiniLabelWhenPossible)
					{
						var topRect = rect;
						var bottomRect = rect;

						topRect.yMax = RawRect.yMax - RawRect.height / 2f;
						bottomRect.yMin = RawRect.yMin + RawRect.height / 2f;

						if (ml0HasValue)
						{
							ml0.Draw(ref topRect);
						}

						if (ml1HasValue)
						{
							ml1.Draw(ref bottomRect);
						}

						rect.xMin = Mathf.Min(topRect.xMin, bottomRect.xMin);
					}
					else if (ml1HasValue)
					{
						ml1.Draw(ref rect);
					}
					else if (ml0HasValue)
					{
						ml0.Draw(ref rect);
					}

					break;
			}
		}

		private static Single CalcMiniLabelSize()
		{
			Styles.MiniLabelStyle.fontSize = 8;

			return MiniLabelProviders.Length switch
			{
				0 => 0.0F,
				1 => MiniLabelProviders[0].Measure(),
				_ => Math.Max(MiniLabelProviders[0].Measure(), MiniLabelProviders[1].Measure())
			};
		}

		private static void DrawTooltip(Rect rect, Single fullTrailingWidth)
		{
			if (!Preferences.Tooltips || !IsGameObject || !IsRepaintEvent)
			{
				return;
			}

			if (DragSelection != null)
			{
				return;
			}

			rect.xMax = Mathf.Min(fullTrailingWidth, rect.xMin + LabelSize);
			rect.xMin = 0f;

			if (!Utility.ShouldCalculateTooltipAt(rect))
			{
				return;
			}

			var tooltip = new StringBuilder(100);

			tooltip.AppendLine(GameObjectName);
			tooltip.AppendFormat("\nTag: {0}", GameObjectTag);
			tooltip.AppendFormat("\nLayer: {0}", LayerMask.LayerToName(CurrentGameObject.layer));

			if (GameObjectUtility.GetStaticEditorFlags(CurrentGameObject) != 0)
			{
				tooltip.AppendFormat("\nStatic: {0}", Utility.EnumFlagsToString(GameObjectUtility.GetStaticEditorFlags(CurrentGameObject)));
			}

			tooltip.AppendLine();
			tooltip.AppendLine();

			foreach (var component in Components.Where(component => (component is not Transform)))
			{
				tooltip.AppendLine(component ? ObjectNames.GetInspectorTitle(component) : "Missing Component");
			}

			EditorGUI.LabelField(rect, Utility.GetTempGUIContent(null, tooltip.ToString().TrimEnd('\n', '\r')));
		}

		private static void DoSelection(Rect rect)
		{
			if (!Preferences.EnhancedSelectionSupported || !Preferences.EnhancedSelection || Event.current.button != 1)
			{
				DragSelection = null;
				return;
			}

			rect.xMin = 0f;

			switch (Event.current.type)
			{
				case EventType.MouseDrag:
					if (!IsFirstVisible)
					{
						return;
					}

					if (DragSelection == null)
					{
						DragSelection = new List<Object>();
						SelectionStart = Event.current.mousePosition;
						SelectionRect = new Rect();
					}

					SelectionRect = new Rect
					{
						xMin = Mathf.Min(Event.current.mousePosition.x, SelectionStart.x),
						yMin = Mathf.Min(Event.current.mousePosition.y, SelectionStart.y),
						xMax = Mathf.Max(Event.current.mousePosition.x, SelectionStart.x),
						yMax = Mathf.Max(Event.current.mousePosition.y, SelectionStart.y)
					};

					if (Event.current.control || Event.current.command)
					{
						DragSelection.AddRange(Selection.objects);
					}

					Selection.objects = DragSelection.ToArray();
					Event.current.Use();
					break;

				case EventType.MouseUp:
					if (DragSelection != null)
					{
						Event.current.Use();
					}

					DragSelection = null;
					break;

				case EventType.Repaint:
					if (DragSelection == null || !IsFirstVisible)
					{
						break;
					}

					Rect scrollRect;

					if (Event.current.mousePosition.y > FinalRect.y)
					{
						scrollRect = FinalRect;
						scrollRect.y += scrollRect.height;
					}
					else if (Event.current.mousePosition.y < RawRect.y)
					{
						scrollRect = RawRect;
						scrollRect.y -= scrollRect.height;
					}
					else
					{
						break;
					}

					SelectionRect = new Rect
					{
						xMin = Mathf.Min(scrollRect.xMax, SelectionStart.x),
						yMin = Mathf.Min(scrollRect.yMax, SelectionStart.y),
						xMax = Mathf.Max(scrollRect.xMax, SelectionStart.x),
						yMax = Mathf.Max(scrollRect.yMax, SelectionStart.y)
					};

					if (Event.current.control || Event.current.command)
					{
						DragSelection.AddRange(Selection.objects);
					}

					Selection.objects = DragSelection.ToArray();

					GUI.ScrollTowards(scrollRect, 9f);
					EditorApplication.RepaintHierarchyWindow();
					break;

				case EventType.Layout:
					if (DragSelection != null && IsGameObject)
					{
						if (!SelectionRect.Overlaps(rect) && DragSelection.Contains(CurrentGameObject))
						{
							DragSelection.Remove(CurrentGameObject);
						}
						else if (SelectionRect.Overlaps(rect) && !DragSelection.Contains(CurrentGameObject))
						{
							DragSelection.Add(CurrentGameObject);
						}
					}

					break;
			}
		}

		private static Color GetRowTint()
		{
			return GetRowTint(RawRect);
		}

		private static Color GetRowTint(Rect rect)
		{
			return ((rect.y / RawRect.height % 2 >= 0.5F) ? Preferences.OddRowColor : Preferences.EvenRowColor);
		}

		public static List<GameObject> GetSelectedObjectsAndCurrent()
		{
			if (!Preferences.ChangeAllSelected || Selection.gameObjects.Length <= 1)
			{
				return new List<GameObject>
				{
					CurrentGameObject,
				};
			}

			var selection = new List<GameObject>(Selection.gameObjects);

			for (var i = 0; i < selection.Count; i++)
				if (EditorUtility.IsPersistent(selection[i]))
				{
					selection.RemoveAt(i);
				}

			if (!selection.Contains(CurrentGameObject))
			{
				selection.Add(CurrentGameObject);
			}

			selection.Remove(null);
			return selection;
		}
	}
}