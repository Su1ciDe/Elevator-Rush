using System.Linq;
using System.Collections;
using Fiber.Managers;
using Fiber.Utilities;
using ScriptableObjects;
using TriInspector;
using UnityEngine;
using Utilities;
using AYellowpaper.SerializedCollections;
using GamePlay.People;
using LevelEditor;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managers
{
	public class PeopleManager : Singleton<PeopleManager>
	{
		[Title("Properties")]
		[SerializeField, ReadOnly] private SerializedDictionary<int, PersonGroup> groups = new();
		public SerializedDictionary<int, PersonGroup> Groups => groups;

		[Space]
		[SerializeField] private Person personPrefab;
		[SerializeField] private PersonGroup personGroupPrefab;
		[SerializeField] private PersonDataSO personDataSO;
		public PersonDataSO PersonDataSO => personDataSO;

		private Coroutine movementCoroutine;
		private Coroutine waitForPeopleMovementCoroutine;

		public PersonGroup LastEnteredGroup { get; set; }

		public static event UnityAction OnMovementCompleted;

		public void WaitMovementElevator(PersonGroup group)
		{
			if (waitForPeopleMovementCoroutine is not null)
			{
				StopCoroutine(waitForPeopleMovementCoroutine);
				waitForPeopleMovementCoroutine = null;
			}

			waitForPeopleMovementCoroutine = StartCoroutine(WaitForPeopleMovementCoroutine(group));
		}

		private IEnumerator WaitForPeopleMovementCoroutine(PersonGroup group)
		{
			yield return movementCoroutine = StartCoroutine(WaitMovementCoroutine(group));

			if (LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator)
			{
				yield return LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator.WaitForPeopleCompleteMovement();
				LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator.CheckIfCompleted();
			}
		}

		public void WaitMovement(PersonGroup group)
		{
			if (movementCoroutine is not null)
			{
				StopCoroutine(movementCoroutine);
				movementCoroutine = null;
			}

			movementCoroutine = StartCoroutine(WaitMovementCoroutine(group));
		}

		private IEnumerator WaitMovementCoroutine(PersonGroup group)
		{
			yield return new WaitUntil(() => !group.People.Any(x => x.IsMoving));

			OnMovementCompleted?.Invoke();
		}

		public void StopWaiting()
		{
			if (waitForPeopleMovementCoroutine is not null)
			{
				StopCoroutine(waitForPeopleMovementCoroutine);
				waitForPeopleMovementCoroutine = null;
			}

			if (movementCoroutine is not null)
			{
				StopCoroutine(movementCoroutine);
				movementCoroutine = null;
			}
		}

#if UNITY_EDITOR
		public Person SpawnPerson(PersonType personType, Direction direction, int groupNo, int x, int y, Vector3 position)
		{
			var person = (Person)PrefabUtility.InstantiatePrefab(personPrefab);
			person.Setup(personType, personDataSO.PersonData[personType].PersonMaterial, direction, new Vector2Int(x, y), position);

			if (!groups.TryGetValue(groupNo, out var group))
			{
				group = (PersonGroup)PrefabUtility.InstantiatePrefab(personGroupPrefab, transform);
				groups.Add(groupNo, group);
			}

			group.AddPerson(person, groupNo);
			return person;
		}

		public void SetupGroups()
		{
			foreach (var group in groups.Values)
				group.SetupGroup();
		}
#endif
	}
}