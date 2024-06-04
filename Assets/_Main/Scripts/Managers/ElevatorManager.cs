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
using Fiber.AudioSystem;
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

		private int value;

		private const float MOVE_SPEED = 20f;

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

			value = (int)currentTempElevator.ElevatorData.Value;
			SetValueText(value);

			currentTempElevator.transform.DOMove(floorPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutBack).OnComplete(() =>
			{
				AudioManager.Instance.PlayAudio(AudioName.Elevator);
				
				OpenDoors();

				CurrentElevator = currentTempElevator;
				OnNewElevator?.Invoke(CurrentElevator);
			});

			if (CurrentElevatorStageIndex + 1 < Elevators.Count)
			{
				var nextElevator = Elevators[CurrentElevatorStageIndex + 1];
				ChangeNextColor(nextElevator.ElevatorData.ElevatorType);
				nextElevator.gameObject.SetActive(true);
				nextElevator.transform.position = moveInPoint.position;
				nextElevator.transform.DOMove(nextElevatorPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutBack);
			}
			else
			{
				ChangeNextColor(PersonType.None);
			}
		}

		public void CompleteStage()
		{
			var tempElevator = CurrentElevator;

			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.Success);

			// Next Stage
			CurrentElevatorStageIndex++;

			CloseDoors();
			DOVirtual.DelayedCall(closeDuration, () =>
			{
				tempElevator.transform.DOMove(moveOutPoint.position, MOVE_SPEED).SetSpeedBased(true).SetEase(Ease.OutQuint).OnComplete(() => Destroy(tempElevator.gameObject));

				if (CurrentElevatorStageIndex < Elevators.Count)
					LoadNewElevator();
				else
					LevelManager.Instance.Win();
			});
		}

		public void CalculateValue()
		{
			value--;
			SetValueText(value);
		}

		public void SetValueText(int _value)
		{
			txtValue.transform.DOComplete();
			txtValue.transform.DOPunchScale(.5f * Vector3.one, 0.2f).SetEase(Ease.InOutExpo);
			txtValue.SetText(_value.ToString());
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
			var mat = PeopleManager.Instance.PersonDataSO.PersonData[type].ElevatorMaterial;
			for (int i = 0; i < meshRenderers.Length; i++)
			{
				if (meshRenderers[i].materials.Length > 1)
				{
					meshRenderers[i].materials[1].color = mat.color;
					Debug.Log(meshRenderers[i].gameObject.name);
				}
				else 
					meshRenderers[i].material = mat;
			}
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
			HapticManager.Instance.PlayHaptic(HapticPatterns.PresetType.Success);
			
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