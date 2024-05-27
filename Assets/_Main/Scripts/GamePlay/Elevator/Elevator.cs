using System.Collections.Generic;
using DG.Tweening;
using Fiber.Managers;
using GamePlay.People;
using Model;
using ScriptableObjects;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay.Elevator
{
	public class Elevator : SlotHolder
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
				meshRenderer.material = personDataSO.PersonData[ElevatorData.ElevatorType];
			}
		}

		public override void MoveToSlot(Person person, Sequence sequence)
		{
			sequence.Append(person.MoveTo(entrancePoint.position, .75f));

			base.MoveToSlot(person, sequence);

			sequence.AppendCallback(() =>
			{
				LevelManager.Instance.CurrentLevel.ElevatorManager.SetValueText((int)ElevatorData.Value - GetPeopleCount());
				person.transform.SetParent(transform);
			});
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