using Interfaces;
using LevelEditor;
using TriInspector;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class BaseObstacle : MonoBehaviour, INode
	{
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[field: SerializeField, ReadOnly] public Direction Direction { get; private set; }

		public void Setup(int x, int y, Vector3 position, Direction direction)
		{
			Coordinates = new Vector2Int(x, y);
			transform.position = position;
			Direction = direction;
			transform.eulerAngles = new Vector3(0, (int)direction * 90);
		}
	}
}