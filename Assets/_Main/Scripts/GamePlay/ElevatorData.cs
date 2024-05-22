using LevelEditor;
using Utilities;

namespace GamePlay
{
	[System.Serializable]
	public class ElevatorData
	{
		public PersonType ElevatorType = PersonType.None;
		public ElevatorValueType Value;

		public ElevatorData(PersonType elevatorType, ElevatorValueType value)
		{
			Value = value;
			ElevatorType = elevatorType;
		}

		public ElevatorData()
		{
			ElevatorType = PersonType.None;
			Value = ElevatorValueType._10;
		}
	}
}