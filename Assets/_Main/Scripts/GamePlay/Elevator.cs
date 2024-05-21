using TriInspector;
using UnityEngine;
using Utilities;

namespace GamePlay
{
	public class Elevator : MonoBehaviour
	{
		[field: SerializeField, ReadOnly] public PersonType ElevatorType { get; private set; }
		[field: SerializeField, ReadOnly] public int Value { get; private set; }
		
		public void Setup(PersonType elevatorType)
		{
			ElevatorType = elevatorType;
		}
	}
}