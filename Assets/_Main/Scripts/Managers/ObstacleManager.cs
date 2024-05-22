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
		[field:Title("Properties")]
		[field: SerializeField, ReadOnly] public List<BaseObstacle> Obstacles { get; private set; } = new List<BaseObstacle>();

#if UNITY_EDITOR
		public BaseObstacle SpawnObstacle(BaseObstacle obstaclePrefab,int x , int y , Vector3 position)
		{
			var obstacle = (BaseObstacle)PrefabUtility.InstantiatePrefab(obstaclePrefab, transform);
			obstacle.Setup(x,y,position);
			Obstacles.Add(obstacle);
			return obstacle;
		}
#endif
	}
}