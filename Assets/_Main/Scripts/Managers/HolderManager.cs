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
				if (!holders[i].CurrentPersonGroup) continue;
				var elevator = LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator;
				if (holders[i].CurrentPersonGroup.Type != elevator.ElevatorData.ElevatorType) continue;

				float duration = 0;
				for (var j = 0; j < holders[i].CurrentPersonGroup.People.Count; j++)
				{
					var person = holders[i].CurrentPersonGroup.People[j];
					duration = person.MoveToSlot(null, elevator).Duration();
					holders[i].Slots[j].CurrentPerson = null;
				}

				PeopleManager.Instance.WaitMovementElevator(holders[i].CurrentPersonGroup, duration + 1f);

				yield return new WaitForSeconds(0.5f);

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
					person.MoveToSlot(new[] { holders[i].CurrentPersonGroup.People[j].transform.position, holders[holderIndex].transform.position }, holders[holderIndex]);
				}

				holderIndex++;
			}
		}

		public void CheckIfFull()
		{
			if (checkIfFullCoroutine == null)
			{
				checkIfFullCoroutine = StartCoroutine(CheckIfFullCoroutine());
			}
		}

		private Coroutine checkIfFullCoroutine;

		private IEnumerator CheckIfFullCoroutine()
		{
			yield return new WaitUntil(() => LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator);
			yield return new WaitForSeconds(.5f);

			if (GetFirstEmptyHolder() is null)
			{
				LevelManager.Instance.Lose();
			}

			checkIfFullCoroutine = null;
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