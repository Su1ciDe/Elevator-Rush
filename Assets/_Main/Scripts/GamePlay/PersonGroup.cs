using System.Collections.Generic;
using TriInspector;
using UnityEngine;
using Utilities;

namespace GamePlay
{
	public class PersonGroup : MonoBehaviour
	{
		[SerializeField, ReadOnly] private PersonType type;
		[SerializeField, ReadOnly] private List<Person> people = new List<Person>();
		public List<Person> People => people;
	}
}