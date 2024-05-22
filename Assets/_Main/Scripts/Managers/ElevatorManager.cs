using System.Collections.Generic;
using GamePlay;
using LevelEditor;
using ScriptableObjects;
using TriInspector;
using UnityEditor;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace Managers
{
	public class ElevatorManager : MonoBehaviour
	{
		[field:Title("Properties")]
		[field: SerializeField, ReadOnly] public List<Elevator> Elevators { get; private set; } = new List<Elevator>();

		[Space]
		[SerializeField] private SerializedDictionary<ElevatorValueType, Elevator> elevatorPrefabs;
		[Space]
		[SerializeField] private PersonDataSO personDataSO;


#if UNITY_EDITOR
		public void Setup(List<ElevatorData> elevatorDatas)
		{
			foreach (var elevatorData in elevatorDatas)
			{
				var elevator = (Elevator)PrefabUtility.InstantiatePrefab(elevatorPrefabs[elevatorData.Value], transform);
				elevator.Setup(elevatorData,personDataSO);

				Elevators.Add(elevator);
			}
		}
#endif
	}
}