using DG.Tweening;
using Model;
using ScriptableObjects;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;

namespace GamePlay
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
			sequence.Append(person.MoveTo(entrancePoint.position));

			base.MoveToSlot(person, sequence);

			sequence.AppendCallback(() =>
			{
				person.transform.SetParent(transform);
				CheckIfCompleted();
			});

			sequence.Append(person.transform.DORotate(180 * Vector3.up, .15f).SetEase(Ease.InOutSine));
		}

		private void CheckIfCompleted()
		{
			Debug.Log("check");
			var totalCount = GetPeopleCount();
			if (totalCount >= (int)ElevatorData.Value)
			{
				Debug.Log("Completed");
				OnComplete?.Invoke(this);
			}
		}
	}
}