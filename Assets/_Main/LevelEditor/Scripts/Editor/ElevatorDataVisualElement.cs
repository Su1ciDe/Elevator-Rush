using UnityEngine.UIElements;
using Utilities;

namespace LevelEditor.Editor
{
	public class ElevatorDataVisualElement : VisualElement
	{
		public ElevatorDataVisualElement()
		{
			var root = new VisualElement { style = { flexGrow = 1, flexDirection = FlexDirection.Row } };

			// Color enum
			var colorEnum = new EnumField { name = "enum_Color", label = "Color", style = { flexGrow = 1 } };
			colorEnum.Init(PersonType.None);

			// Value Slider
			var valueEnum = new EnumField() { name = "enum_Value", label = "Value", style = { flexGrow = 1 } };
			valueEnum.Init(ElevatorValueType._10);

			root.Add(colorEnum);
			root.Add(valueEnum);

			Add(root);
		}
	}
}