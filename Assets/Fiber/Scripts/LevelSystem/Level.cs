using System.Linq;
using System.Collections;
using Fiber.Managers;
using GamePlay.Elevator;
using Managers;
using UnityEngine;
using Grid = GridSystem.Grid;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[SerializeField] private Grid grid;
		public Grid Grid => grid;

		[SerializeField] private PeopleManager peopleManager;
		public PeopleManager PeopleManager => peopleManager;

		[SerializeField] private ElevatorManager elevatorManager;
		public ElevatorManager ElevatorManager => elevatorManager;

		[SerializeField] private ObstacleManager obstacleManager;
		public ObstacleManager ObstacleManager => obstacleManager;

		[SerializeField] private HolderManager holderManager;
		public HolderManager HolderManager => holderManager;

		private void OnEnable()
		{
			PeopleManager.OnMovementCompleted += FailCheck;

			// ElevatorManager.OnNewElevator += OnNewElevatorFirst;
			// PeopleManager.OnMovementCompleted += OnMovementCompleted;
		}

		private void OnDisable()
		{
			PeopleManager.OnMovementCompleted -= FailCheck;

			// ElevatorManager.OnNewElevator -= OnNewElevatorFirst;
			// ElevatorManager.OnNewElevator -= OnNewElevator;
			// PeopleManager.OnMovementCompleted -= OnMovementCompleted;
		}

		public virtual void Load()
		{
			gameObject.SetActive(true);
		}

		public virtual void Play()
		{
		}

		// private void OnMovementCompleted()
		// {
		// 	CheckFail();
		// }

		// private void OnNewElevatorFirst(Elevator elevator)
		// {
		// 	ElevatorManager.OnNewElevator -= OnNewElevatorFirst;
		// 	ElevatorManager.OnNewElevator += OnNewElevator;
		// }

		private void OnNewElevator(Elevator elevator)
		{
			// CheckFail();
		}

		// public void CheckFail()
		// {
		// 	if (checkFailCoroutine is not null)
		// 		StopCoroutine(checkFailCoroutine);
		//
		// 	checkFailCoroutine = StartCoroutine(FailCheckCoroutine());
		// }

		private Coroutine checkFailCoroutine;

		private IEnumerator CheckFailCoroutine()
		{
			yield return new WaitForSeconds(1f);
			yield return new WaitUntil(() => elevatorManager.CurrentElevator);

			var currentElevator = elevatorManager.CurrentElevator;
			bool peopleCanMove = false;
			foreach (var personGroup in PeopleManager.Instance.Groups.Values)
			{
				if (personGroup.People.Any(x => x.IsMoving)) yield break;
				// yield return new WaitUntil(() => !personGroup.People.Any(x => x.IsMoving));
				yield return null;
				if (personGroup.IsCompleted) continue;

				var leader = personGroup.People[0];
				// if (leader.IsMoving) continue;
				// yield return new WaitUntil(() => !leader.IsMoving);
				if (currentElevator != elevatorManager.CurrentElevator) yield break;

				if (leader.PersonType != ElevatorManager.CurrentElevator?.ElevatorData.ElevatorType) continue;

				var path = leader.CheckPath();
				if ((path is not null && path.Count > 0) || HolderManager.GetFirstEmptyHolder() is not null)
				{
					peopleCanMove = true;
					break;
				}
				else if (leader.CurrentCell.CurrentPerson != leader) // means that the group is in the holder
				{
					peopleCanMove = true;
					break;
				}
			}

			if (!peopleCanMove)
			{
				LevelManager.Instance.Lose();
			}
		}

		private void FailCheck()
		{
			if (checkFailCoroutine is not null)
				StopCoroutine(checkFailCoroutine);

			checkFailCoroutine = StartCoroutine(FailCheckCoroutine());
		}

		private IEnumerator FailCheckCoroutine()
		{
			// yield return new WaitForSeconds(0.5f);
			// yield return new WaitUntil(() => elevatorManager.CurrentElevator);
			// yield return null;
			//
			// foreach (var personGroup in PeopleManager.Instance.Groups.Values)
			// 	yield return new WaitUntil(() => !personGroup.People.Any(x => x.IsMoving));
			
			yield return null;

			if (HolderManager.GetFirstEmptyHolder() is null)
				LevelManager.Instance.Lose();

			checkFailCoroutine = null;
		}
	}
}