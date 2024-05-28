using AYellowpaper.SerializedCollections;
using UnityEngine;
using Utilities;

namespace ScriptableObjects
{
	[CreateAssetMenu(fileName = "PersonData", menuName = "Elevator Rush/Person Data", order = 0)]
	public class PersonDataSO : ScriptableObject
	{
		[System.Serializable]
		public class MaterialData
		{
			public Material PersonMaterial;
			public Material ElevatorMaterial;
		}

		public SerializedDictionary<PersonType, MaterialData> PersonData = new SerializedDictionary<PersonType, MaterialData>();
	}
}