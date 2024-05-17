using GamePlay;
using GamePlay.Obstacles;
using TriInspector;
using UnityEngine;

namespace GridSystem
{
	[SelectionBase]
	public class GridCell : MonoBehaviour
	{
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; private set; }

		[field: SerializeField, ReadOnly] public BaseObstacle CurrentObstacle { get; set; }
		[field: SerializeField, ReadOnly] public Person CurrentPerson { get; set; }

		public void Setup(int x, int y, Vector2 nodeSize)
		{
			Coordinates = new Vector2Int(x, y);
			transform.localScale = new Vector3(nodeSize.x, 1f, nodeSize.y);
		}
	}
}