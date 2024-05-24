using Managers;
using UnityEngine;
using Grid = GridSystem.Grid;

namespace Fiber.LevelSystem
{
	public class Level : MonoBehaviour
	{
		[SerializeField] private Grid grid;
		public Grid Grid => grid;

		[SerializeField] private PeopleManager peopleManager;
		public PeopleManager PeopleManager => peopleManager;

		[SerializeField] private ElevatorManager elevatorManager;
		public ElevatorManager ElevatorManager => elevatorManager;

		[SerializeField] private ObstacleManager obstacleManager;
		public ObstacleManager ObstacleManager => obstacleManager;

		[SerializeField] private HolderManager holderManager;
		public HolderManager HolderManager => holderManager;

		public virtual void Load()
		{
			gameObject.SetActive(true);
		}

		public virtual void Play()
		{
		}
	}
}