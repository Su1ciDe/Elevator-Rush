using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fiber.Managers;
using GamePlay;
using GamePlay.Elevator;
using TriInspector;
using UnityEngine;

namespace Managers
{
	public class HolderManager : MonoBehaviour
	{
		[Title("Properties")]
		[SerializeField] private int amount;
		public int Amount => amount;
		[SerializeField] private float spacing = 0;

		[Space]
		[SerializeField] private Holder holderPrefab;

		private readonly List<Holder> holders = new List<Holder>();

		private const float SIZE = 1;

		private void Awake()
		{
			Init();
		}

		private void OnEnable()
		{
			ElevatorManager.OnNewElevator += OnNewElevator;
		}

		private void OnDisable()
		{
			ElevatorManager.OnNewElevator -= OnNewElevator;
		}

		private void OnNewElevator(Elevator newElevator)
		{
			CheckPeople();
		}

		private void Init()
		{
			var offset = amount * SIZE / 2f - SIZE / 2f + spacing * ((amount - 1) / 2f);
			for (int i = 0; i < amount; i++)
			{
				var holder = Instantiate(holderPrefab, transform);
				holder.transform.localPosition = new Vector3(i * SIZE + spacing * i - offset, 0, 0);
				holders.Add(holder);
			}
		}

		public void CheckPeople()
		{
			StartCoroutine(CheckPeopleCoroutine());
		}

		private IEnumerator CheckPeopleCoroutine()
		{
			for (var i = 0; i < holders.Count; i++)
			{
				var group = holders[i].CurrentPersonGroup;
				if (!group) continue;
				var elevator = LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator;
				if (group.Type != elevator.ElevatorData.ElevatorType) continue;
				if (elevator.GetFirstEmptySlot() is null) yield break;

				for (var j = 0; j < group.People.Count; j++)
				{
					var person = group.People[j];
					holders[i].Slots[j].CurrentPerson = null;
					var i1 = i;
					person.MoveToSlot(null, elevator, j).onComplete += () =>
					{
						if ((i1 + 1).Equals(group.People.Count) && PeopleManager.Instance.LastEnteredGroup == group)
							WaitMovement();
					};
				}

				PeopleManager.Instance.LastEnteredGroup = group;
				PeopleManager.Instance.StopWaiting();

				void WaitMovement()
				{
					PeopleManager.Instance.WaitMovementElevator(group);
				}

				yield return new WaitForSeconds(0.55f);
				holders[i].CurrentPersonGroup = null;
			}

			// RearrangePeople();
		}

		public void RearrangePeople()
		{
			var holderIndex = 0;
			for (int i = 0; i < holders.Count; i++)
			{
				if (!holders[i].CurrentPersonGroup) continue;

				for (var j = 0; j < holders[i].CurrentPersonGroup.People.Count; j++)
				{
					var person = holders[i].CurrentPersonGroup.People[j];
					person.MoveToSlot(new List<Vector3> { holders[i].CurrentPersonGroup.People[j].transform.position, holders[holderIndex].transform.position }, holders[holderIndex], j);
				}

				holderIndex++;
			}
		}

		public Holder GetFirstEmptyHolder()
		{
			for (int i = 0; i < holders.Count; i++)
			{
				if (!holders[i].CurrentPersonGroup)
					return holders[i];
			}

			return null;
		}

		#region Setup

#if UNITY_EDITOR
		public void Setup(int _amount)
		{
			amount = _amount;
		}
#endif

		#endregion
	}
}