using System.Collections.Generic;
using GamePlay;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace Managers
{
	public class ElevatorManager : MonoBehaviour
	{
		[field: SerializeField, ReadOnly] public List<Elevator> Elevators { get; private set; } = new List<Elevator>();

		[SerializeField] private Elevator elevatorPrefab;

#if UNITY_EDITOR
		public void Setup(List<PersonType> elevatorTypes)
		{
			foreach (var elevatorType in elevatorTypes)
			{
				var elevator = (Elevator)PrefabUtility.InstantiatePrefab(elevatorPrefab, transform);
				elevator.Setup(elevatorType);

				Elevators.Add(elevator);
			}
		}
#endif
	}
}