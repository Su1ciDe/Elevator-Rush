using DG.Tweening;
using UnityEngine;

namespace GamePlay.Elevator
{
	public class ElevatorDoor : MonoBehaviour
	{
		[SerializeField] private Transform closePoint;
		[SerializeField] private Transform openPoint;

		public void Open(float duration)
		{
			transform.DOMove(openPoint.position, duration).SetEase(Ease.InOutCubic);
		}

		public void Close(float duration)
		{
			transform.DOMove(closePoint.position, duration).SetEase(Ease.InOutCubic);
		}
	}
}