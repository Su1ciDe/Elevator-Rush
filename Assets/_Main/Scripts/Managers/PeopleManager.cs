using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fiber.Utilities;
using GamePlay;
using ScriptableObjects;
using TriInspector;
using UnityEngine;
using Utilities;
using AYellowpaper.SerializedCollections;
using Fiber.Managers;
using GamePlay.People;
using LevelEditor;
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

		private Coroutine waitForPeopleMovementCoroutine;

		private IEnumerator WaitForPeopleMovementCoroutine(List<Person> people, float delay)
		{
			yield return new WaitForSeconds(delay);
			
			if (!people.Any(x => x.IsMoving))
				LevelManager.Instance.CurrentLevel.ElevatorManager.CurrentElevator?.CheckIfCompleted();
		}

		public void WaitForPeopleMovement(List<Person> people, float delay)
		{
			if (waitForPeopleMovementCoroutine is not null)
			{
				StopCoroutine(waitForPeopleMovementCoroutine);
				waitForPeopleMovementCoroutine = null;
			}

			waitForPeopleMovementCoroutine = StartCoroutine(WaitForPeopleMovementCoroutine(people, delay));
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

			group.AddPerson(person);
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