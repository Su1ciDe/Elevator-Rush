using GamePlay.Obstacles;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace LevelEditor
{
	public class CellInfo
	{
		public Vector2Int Coordinates;
		public Button Button;
		public Color Color;

		public PersonType PersonType = PersonType.None;
		public int GroupNo;
		public Direction Direction;
		
		public BaseObstacle Obstacle;
	}
}