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

		private Coroutine waitForPeopleMovementCoroutine;

		public static event UnityAction OnMovementCompleted;

		public void WaitMovementElevator(PersonGroup group, float delay)
		{
			if (waitForPeopleMovementCoroutine is not null)
			{
				StopCoroutine(waitForPeopleMovementCoroutine);
				waitForPeopleMovementCoroutine = null;
			}

			waitForPeopleMovementCoroutine = StartCoroutine(WaitForPeopleMovementCoroutine(group, delay));
		}

		private IEnumerator WaitForPeopleMovementCoroutine(PersonGroup group, float delay)
		{
			yield return WaitMovementCoroutine(group, delay);

			if (LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator)
			{
				LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator.CheckIfCompleted();
				group.IsCompleted = true;
			}
		}

		public void WaitMovement(PersonGroup group, float delay)
		{
			StartCoroutine(WaitMovementCoroutine(group, delay));
		}

		private IEnumerator WaitMovementCoroutine(PersonGroup group, float delay)
		{
			yield return new WaitForSeconds(delay);
			yield return new WaitUntil(() => !group.People.Any(x => x.IsMoving));

			OnMovementCompleted?.Invoke();
		}

#if UNITY_EDITOR
		public Person SpawnPerson(PersonType personType, Direction direction, int groupNo, int x, int y, Vector3 position)
		{
			var person = (Person)PrefabUtility.InstantiatePrefab(personPrefab);
			person.Setup(personType, personDataSO.PersonData[personType], direction, new Vector2Int(x, y), position);

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