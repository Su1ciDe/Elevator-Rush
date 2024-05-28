using GamePlay.People;
using Model;
using ScriptableObjects;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Elevator
{
	public class Elevator : PersonSlotController
	{
		[Title("Properties")]
		[SerializeField, ReadOnly] public ElevatorData ElevatorData;

		[Space]
		[SerializeField] private MeshRenderer[] meshRenderers;

		[Space]
		[SerializeField] private Transform entrancePoint;

		public static event UnityAction<Elevator> OnComplete;

		public void Setup(ElevatorData elevatorData, PersonDataSO personDataSO)
		{
			ElevatorData = elevatorData;

			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material = personDataSO.PersonData[ElevatorData.ElevatorType].ElevatorMaterial;
			}
		}

		public override PersonSlot MoveToSlot(Person person)
		{
			person.PathList.Add(entrancePoint.position);

			return base.MoveToSlot(person);
		}

		public bool CheckIfCompleted()
		{
			var totalCount = GetPeopleCount();
			if (totalCount.Equals((int)ElevatorData.Value))
			{
				OnComplete?.Invoke(this);
				return true;
			}

			return false;
		}
	}
}