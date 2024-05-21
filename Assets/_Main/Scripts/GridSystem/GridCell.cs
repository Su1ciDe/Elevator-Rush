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
		public int X => Coordinates.x;
		public int Y => Coordinates.y;

		[field: SerializeField, ReadOnly] public BaseObstacle CurrentObstacle { get; set; }
		[field: SerializeField, ReadOnly] public Person CurrentPerson { get; set; }
		
		// Pathfinding
		public int GCost { get; set; }
		public int HCost { get; set; }
		public int FCost { get; set; }
		public GridCell PreviousCell { get; set; }
		
		public void Setup(int x, int y, Vector2 nodeSize)
		{
			Coordinates = new Vector2Int(x, y);
			transform.localScale = new Vector3(nodeSize.x, 1f, nodeSize.y);
		}

		public int CalculateFCost()
		{
			FCost = GCost + HCost;
			return FCost;
		}
	}
}