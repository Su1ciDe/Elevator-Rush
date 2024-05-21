using Fiber.Utilities;
using GamePlay;
using ScriptableObjects;
using TriInspector;
using UnityEngine;
using Utilities;
using AYellowpaper.SerializedCollections;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managers
{
	public class PeopleManager : Singleton<PeopleManager>
	{
		[SerializeField] private Person personPrefab;
		[SerializeField] private PersonGroup personGroupPrefab;
		[SerializeField] private PersonDataSO personDataSO;

		[SerializeField, ReadOnly] private SerializedDictionary<int, PersonGroup> groups = new();
		public SerializedDictionary<int, PersonGroup> Groups => groups;

#if UNITY_EDITOR
		public void SpawnPerson(PersonType personType, int groupNo, int x, int y, Vector3 position)
		{
			var person = (Person)PrefabUtility.InstantiatePrefab(personPrefab);
			person.Setup(personType, personDataSO.PersonData[personType], new Vector2Int(x, y), position);

			if (!groups.TryGetValue(groupNo, out var group))
			{
				group = (PersonGroup)PrefabUtility.InstantiatePrefab(personGroupPrefab, transform);
				groups.Add(groupNo, group);
			}

			group.AddPerson(person);
		}
#endif
	}
}