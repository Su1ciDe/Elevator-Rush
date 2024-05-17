using System.Collections.Generic;
using Fiber.Utilities;
using GamePlay;
using ScriptableObjects;
using TriInspector;
using UnityEditor;
using UnityEngine;
using Utilities;

namespace Managers
{
	public class PeopleManager : Singleton<PeopleManager>
	{
		[SerializeField] private Person personPrefab;
		[SerializeField] private PersonGroup personGroupPrefab;
		[SerializeField] private PersonDataSO personDataSO;

		[SerializeField, ReadOnly] private List<PersonGroup> groups = new List<PersonGroup>();
		public List<PersonGroup> Groups => groups;

#if UNITY_EDITOR
		public void SpawnPerson(PersonType personType, int x, int y)
		{
			var person = (Person)PrefabUtility.InstantiatePrefab(personPrefab);
		}

		public void SetupGroups()
		{
		}
#endif
	}
}