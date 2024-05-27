using System.Collections;
using System.Collections.Generic;
using LevelEditor;
using ScriptableObjects;
using TriInspector;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using Fiber.Managers;
using Lofelt.NiceVibrations;
using AYellowpaper.SerializedCollections;
using GamePlay.Elevator;
using TMPro;
using UnityEngine.Events;
using Utilities;

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

		[Title("Elevator")]
		[SerializeField] private ElevatorDoor leftDoor;
		[SerializeField] private ElevatorDoor rightDoor;
		[SerializeField] private float openDuration = .25f;
		[SerializeField] private float closeDuration = .25f;
		[Space]
		[SerializeField] private TextMeshPro txtValue;
		[SerializeField] private MeshRenderer[] currentElevatorMRs;
		[SerializeField] private MeshRenderer[] nextElevatorMRs;

		[Title("Points")]
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
			ChangeCurrentColor(currentTempElevator.ElevatorData.ElevatorType);
			SetValueText((int)currentTempElevator.ElevatorData.Value);
			currentTempElevator.transform.DOMove(floorPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutBack).OnComplete(() =>
			{
				// Open Doors
				OpenDoors();

				CurrentElevator = currentTempElevator;
				OnNewElevator?.Invoke(CurrentElevator);

				// Check if can play
				// CheckIfFailed();
			});

			if (CurrentElevatorStageIndex + 1 < Elevators.Count)
			{
				var nextElevator = Elevators[CurrentElevatorStageIndex + 1];
				ChangeNextColor(nextElevator.ElevatorData.ElevatorType);
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
				CloseDoors();

				tempElevator.transform.DOMove(moveOutPoint.position, MOVE_SPEED).SetDelay(closeDuration).SetSpeedBased(true).SetEase(Ease.OutQuint).OnComplete(() => Destroy(tempElevator.gameObject));
				LoadNewElevator();
			}
			else
			{
				LevelManager.Instance.Win();
			}
		}

		public void SetValueText(int value)
		{
			txtValue.SetText(value.ToString());
		}

		private void ChangeCurrentColor(PersonType type)
		{
			ChangeMaterial(currentElevatorMRs, type);
		}

		private void ChangeNextColor(PersonType type)
		{
			ChangeMaterial(nextElevatorMRs, type);
		}

		private void ChangeMaterial(MeshRenderer[] meshRenderers, PersonType type)
		{
			var mat = PeopleManager.Instance.PersonDataSO.PersonData[type];
			for (int i = 0; i < meshRenderers.Length; i++)
				meshRenderers[i].material = mat;
		}

		private void OpenDoors()
		{
			rightDoor.Open(openDuration);
			leftDoor.Open(openDuration);
		}

		private void CloseDoors()
		{
			rightDoor.Close(closeDuration);
			leftDoor.Close(closeDuration);
		}

		private void OnElevatorCompleted(Elevator elevator)
		{
			CompleteStage();
		}

		public void CheckIfFailed()
		{
			if (checkIfCanPlayCoroutine is not null)
				StopCoroutine(checkIfCanPlayCoroutine);

			checkIfCanPlayCoroutine = StartCoroutine(CheckIfCanPlayCoroutine());
		}

		private Coroutine checkIfCanPlayCoroutine;

		private IEnumerator CheckIfCanPlayCoroutine()
		{
			yield return new WaitForSeconds(0.5f);

			bool peopleCanMove = false;
			foreach (var personGroup in PeopleManager.Instance.Groups.Values)
			{
				var leader = personGroup.People[0];
				var path = leader.CheckPath();
				if (path is not null)
				{
					peopleCanMove = true;
					break;
				}
			}

			if (!peopleCanMove)
			{
				if (LevelManager.Instance.CurrentLevel.HolderManager.GetFirstEmptyHolder() is null)
				{
					LevelManager.Instance.Lose();
				}
			}
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