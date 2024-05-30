using System.Collections;
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
		[SerializeField] private Transform entrancePoint;

		public static event UnityAction<Elevator> OnComplete;

		public void Setup(ElevatorData elevatorData, PersonDataSO personDataSO)
		{
			ElevatorData = elevatorData;
		}

		public override PersonSlot MoveToSlot(Person person)
		{
			person.PathList.Add(entrancePoint.position);

			return base.MoveToSlot(person);
		}

		public IEnumerator WaitForPeopleCompleteMovement()
		{
			for (var i = 0; i < Slots.Length; i++)
			{
				var i1 = i;
				if (Slots[i1].CurrentPerson)
					yield return new WaitUntil(() => !Slots[i1].CurrentPerson.IsMoving);
			}
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