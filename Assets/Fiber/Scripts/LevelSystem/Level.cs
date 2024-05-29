using System.Collections;
using System.Linq;
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
			ElevatorManager.OnNewElevator += OnNewElevator;
			PeopleManager.OnMovementCompleted += OnMovementCompleted;
		}

		private void OnDisable()
		{
			ElevatorManager.OnNewElevator -= OnNewElevator;
			PeopleManager.OnMovementCompleted -= OnMovementCompleted;
		}

		public virtual void Load()
		{
			gameObject.SetActive(true);
		}

		public virtual void Play()
		{
		}

		private void OnMovementCompleted()
		{
			CheckFail();
		}

		private void OnNewElevator(Elevator elevator)
		{
			CheckFail();
		}

		public void CheckFail()
		{
			if (checkFailCoroutine is not null)
				StopCoroutine(checkFailCoroutine);

			checkFailCoroutine = StartCoroutine(CheckFailCoroutine());
		}

		private Coroutine checkFailCoroutine;

		private IEnumerator CheckFailCoroutine()
		{
			yield return new WaitUntil(() => elevatorManager.CurrentElevator);
			yield return new WaitForSeconds(1);

			bool peopleCanMove = false;
			foreach (var personGroup in PeopleManager.Instance.Groups.Values)
			{
				if (personGroup.IsCompleted) continue;
				// if (personGroup.People.Any(x => x.IsMoving)) yield break;
				// yield return new WaitUntil(() => !personGroup.People.Any(x => x.IsMoving));

				var leader = personGroup.People[0];
				if (leader.IsMoving) continue;
				// yield return new WaitUntil(() => !leader.IsMoving);

				var path = leader.CheckPath();
				if (path is not null && path.Count > 0)
				{
					if (leader.PersonType == ElevatorManager.CurrentElevator?.ElevatorData.ElevatorType || HolderManager.GetFirstEmptyHolder() is not null)
					{
						peopleCanMove = true;
						break;
					}
				}
			}

			if (!peopleCanMove)
			{
				LevelManager.Instance.Lose();
			}
		}
	}
}