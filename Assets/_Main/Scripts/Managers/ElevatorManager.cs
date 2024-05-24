using System.Collections.Generic;
using GamePlay;
using LevelEditor;
using ScriptableObjects;
using TriInspector;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using Fiber.Managers;
using Lofelt.NiceVibrations;
using AYellowpaper.SerializedCollections;
using UnityEngine.Events;

namespace Managers
{
	public class ElevatorManager : MonoBehaviour
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public List<Elevator> Elevators { get; private set; } = new List<Elevator>();

		public Elevator CurrentElevator { get; private set; }
		public int CurrentElevatorStageIndex { get; set; } = 0;

		[Space]
		[SerializeField] private SerializedDictionary<ElevatorValueType, Elevator> elevatorPrefabs;
		[Space]
		[SerializeField] private PersonDataSO personDataSO;
		[Space]
		[SerializeField] private Transform floorPoint;
		[SerializeField] private Transform nextElevatorPoint;
		[SerializeField] private Transform moveInPoint;
		[SerializeField] private Transform moveOutPoint;

		private const float MOVE_SPEED = 15f;

		public static event UnityAction<Elevator> OnNewElevator;

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			Elevator.OnComplete += OnElevatorCompleted;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			Elevator.OnComplete -= OnElevatorCompleted;
		}

		private void OnLevelStarted()
		{
			var currentTempElevator = Elevators[CurrentElevatorStageIndex];
			currentTempElevator.transform.position = nextElevatorPoint.position;
			currentTempElevator.gameObject.SetActive(true);

			LoadNewElevator();
		}

		private void LoadNewElevator()
		{
			CurrentElevator = null;
			var currentTempElevator = Elevators[CurrentElevatorStageIndex];
			currentTempElevator.transform.DOMove(floorPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutBack).OnComplete(() =>
			{
				CurrentElevator = currentTempElevator;
				OnNewElevator?.Invoke(CurrentElevator);
			});

			if (CurrentElevatorStageIndex + 1 < Elevators.Count)

			{
				var nextElevator = Elevators[CurrentElevatorStageIndex + 1];
				nextElevator.gameObject.SetActive(true);
				nextElevator.transform.position = moveInPoint.position;
				nextElevator.transform.DOMove(nextElevatorPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutBack);
			}
		}

		public void CompleteStage()
		{
			var tempElevator = CurrentElevator;

			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.Success);

			// Next Stage
			CurrentElevatorStageIndex++;
			if (CurrentElevatorStageIndex < Elevators.Count)
			{
				//TODO: elevator door
				//

				tempElevator.transform.DOMove(moveOutPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutQuint).OnComplete(() => Destroy(tempElevator.gameObject));
				LoadNewElevator();
			}
			else
			{
				LevelManager.Instance.Win();
			}
		}

		private void OnElevatorCompleted(Elevator elevator)
		{
			CompleteStage();
		}

#if UNITY_EDITOR
		public void Setup(List<ElevatorData> elevatorDatas)
		{
			foreach (var elevatorData in elevatorDatas)
			{
				var elevator = (Elevator)PrefabUtility.InstantiatePrefab(elevatorPrefabs[elevatorData.Value], transform);
				elevator.Setup(elevatorData, personDataSO);

				Elevators.Add(elevator);
				elevator.gameObject.SetActive(false);
			}
		}
#endif
	}
}