using Interfaces;
using TriInspector;
using UnityEngine;

namespace GamePlay.Obstacles
{
	public abstract class BaseObstacle : MonoBehaviour, INode
	{
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }

		public void Setup(int x, int y, Vector3 position)
		{
			Coordinates = new Vector2Int(x, y);
			transform.position = position;
		}
	}
}