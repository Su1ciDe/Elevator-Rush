using ScriptableObjects;
using TriInspector;
using UnityEngine;

namespace GamePlay
{
	public class Elevator : MonoBehaviour
	{
		[SerializeField, ReadOnly] public ElevatorData ElevatorData;

		[SerializeField] private MeshRenderer[] meshRenderers;
 
		public void Setup(ElevatorData elevatorData,PersonDataSO personDataSO)
		{
			ElevatorData = elevatorData;

			foreach (var meshRenderer in meshRenderers)
			{
				meshRenderer.material = personDataSO.PersonData[ElevatorData.ElevatorType];
			}
		}
	}
}