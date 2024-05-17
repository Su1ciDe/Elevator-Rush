using AYellowpaper.SerializedCollections;
using UnityEngine;
using Utilities;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "PersonData", menuName = "Elevator Rush/Person Data", order = 0)]
	public class PersonDataSO : ScriptableObject
	{
		public SerializedDictionary<PersonType, Material> PersonData = new SerializedDictionary<PersonType, Material>();
	}
}