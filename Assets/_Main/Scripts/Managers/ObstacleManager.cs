using System.Collections.Generic;
using Fiber.Utilities;
using GamePlay.Obstacles;
using TriInspector;
using UnityEditor;
using UnityEngine;

namespace Managers
{
	public class ObstacleManager : Singleton<ObstacleManager>
	{
		[field: SerializeField, ReadOnly] public List<BaseObstacle> Obstacles { get; private set; } = new List<BaseObstacle>();

#if UNITY_EDITOR
		public void SpawnObstacle(BaseObstacle obstaclePrefab)
		{
			var obstacle = (BaseObstacle)PrefabUtility.InstantiatePrefab(obstaclePrefab, transform);
		}
#endif
	}
}