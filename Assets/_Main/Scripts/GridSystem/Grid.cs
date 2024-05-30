using System.Collections.Generic;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.Utilities.Extensions;
using LevelEditor;
using Managers;
using TriInspector;
using UnityEngine;
using Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GridSystem
{
	public class Grid : Singleton<Grid>
	{
		[SerializeField] private Vector2 nodeSize = new Vector2Int(1, 1);
		[SerializeField] private float xSpacing = 0;
		[SerializeField] private float ySpacing = 0;
		[SerializeField] private GridCell cellPrefab;

		[ReadOnly]
		[SerializeField] private GridCellMatrix gridCells;
		public GridCellMatrix GridCells => gridCells;

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
		}

		private void OnLevelLoaded()
		{
		}

		#region Pathfinding

		private List<GridCell> openList; // Can further optimize to be a binary tree
		private List<GridCell> closedList;

		private const int MOVE_COST = 10;
		private const int MOVE_COST_DIAGONAL = 14;

		public List<GridCell> FindPath(int startX, int startY, int endX, int endY)
		{
			var startCell = gridCells[startX, startY];
			var endCell = gridCells[endX, endY];

			openList = new List<GridCell> { startCell };
			closedList = new List<GridCell>();

			for (int x = 0; x < gridCells.GetLength(0); x++)
			{
				for (int y = 0; y < gridCells.GetLength(1); y++)
				{
					var cell = gridCells[x, y];
					cell.GCost = int.MaxValue;
					cell.HCost = 0;
					cell.CalculateFCost();
					cell.PreviousCell = null;
				}
			}

			startCell.GCost = 0;
			startCell.HCost = CalculateDistanceCost(startCell, endCell);
			startCell.CalculateFCost();

			while (openList.Count > 0)
			{
				var currentCell = GetLowestFCostNode(openList);
				if (currentCell.Equals(endCell))
					return CalculatePath(endCell);

				openList.Remove(currentCell);
				closedList.Add(currentCell);

				var neighbourList = GetNeighbours(currentCell);
				foreach (var neighbourCell in neighbourList)
				{
					if (closedList.Contains(neighbourCell)) continue;
					if (!neighbourCell.IsWalkable)
					{
						closedList.Add(neighbourCell);
						continue;
					}

					int tentativeGCost = currentCell.GCost + CalculateDistanceCost(currentCell, neighbourCell);
					if (tentativeGCost < neighbourCell.GCost)
					{
						neighbourCell.PreviousCell = currentCell;
						neighbourCell.GCost = tentativeGCost;
						neighbourCell.HCost = CalculateDistanceCost(neighbourCell, endCell);
						neighbourCell.CalculateFCost();

						openList.AddIfNotContains(neighbourCell);
					}
				}
			}

			return null;
		}

		private List<GridCell> CalculatePath(GridCell endCell)
		{
			var path = new List<GridCell> { endCell };
			var currentCell = endCell;
			while (currentCell.PreviousCell)
			{
				path.Add(currentCell.PreviousCell);
				currentCell = currentCell.PreviousCell;
			}

			path.Reverse();

			return path;
		}

		private int CalculateDistanceCost(GridCell a, GridCell b)
		{
			int xDistance = Mathf.Abs(a.X - b.X);
			int yDistance = Mathf.Abs(a.Y - b.Y);
			int remaining = Mathf.Abs(xDistance - yDistance);
			return MOVE_COST_DIAGONAL * Mathf.Min(xDistance, yDistance) + MOVE_COST * remaining;
		}

		private GridCell GetLowestFCostNode(List<GridCell> gridCellList)
		{
			var lowestFCostCell = gridCellList[0];
			for (int i = 0; i < gridCellList.Count; i++)
			{
				if (gridCellList[i].FCost < lowestFCostCell.FCost)
					lowestFCostCell = gridCellList[i];
			}

			return lowestFCostCell;
		}

		#region Neighbours

		public List<GridCell> GetNeighbours(GridCell currentCell)
		{
			var neighbourList = new List<GridCell>();

			// Up
			if (currentCell.Y - 1 >= 0)
				neighbourList.Add(gridCells[currentCell.X, currentCell.Y - 1]);
			// Right
			if (currentCell.X + 1 < GridCells.GetLength(0))
				neighbourList.Add(gridCells[currentCell.X + 1, currentCell.Y]);
			// Down
			if (currentCell.Y + 1 < gridCells.GetLength(1))
				neighbourList.Add(gridCells[currentCell.X, currentCell.Y + 1]);
			// Left 
			if (currentCell.X - 1 >= 0)
				neighbourList.Add(gridCells[currentCell.X - 1, currentCell.Y]);

			return neighbourList;
		}

		public List<GridCell> GetSameNeighbours(GridCell currentCell)
		{
			var neighbourList = new List<GridCell>();

			// Up
			if (currentCell.Y - 1 >= 0 && gridCells[currentCell.X, currentCell.Y - 1].CurrentPerson)
				if (currentCell.CurrentPerson.PersonType == gridCells[currentCell.X, currentCell.Y - 1].CurrentPerson.PersonType &&
				    currentCell.CurrentPerson.GetGroup().GroupNo == gridCells[currentCell.X, currentCell.Y - 1].CurrentPerson.GetGroup().GroupNo)
					neighbourList.Add(gridCells[currentCell.X, currentCell.Y - 1]);
			// Right
			if (currentCell.X + 1 < GridCells.GetLength(0) && gridCells[currentCell.X + 1, currentCell.Y].CurrentPerson)
				if (currentCell.CurrentPerson.PersonType == gridCells[currentCell.X + 1, currentCell.Y].CurrentPerson.PersonType &&
				    currentCell.CurrentPerson.GetGroup().GroupNo == gridCells[currentCell.X + 1, currentCell.Y].CurrentPerson.GetGroup().GroupNo)
					neighbourList.Add(gridCells[currentCell.X + 1, currentCell.Y]);
			// Down
			if (currentCell.Y + 1 < gridCells.GetLength(1) && gridCells[currentCell.X, currentCell.Y + 1].CurrentPerson)
				if (currentCell.CurrentPerson.PersonType == gridCells[currentCell.X, currentCell.Y + 1].CurrentPerson.PersonType &&
				    currentCell.CurrentPerson.GetGroup().GroupNo == gridCells[currentCell.X, currentCell.Y + 1].CurrentPerson.GetGroup().GroupNo)
					neighbourList.Add(gridCells[currentCell.X, currentCell.Y + 1]);
			// Left 
			if (currentCell.X - 1 >= 0 && gridCells[currentCell.X - 1, currentCell.Y].CurrentPerson)
				if (currentCell.CurrentPerson.PersonType == gridCells[currentCell.X - 1, currentCell.Y].CurrentPerson.PersonType &&
				    currentCell.CurrentPerson.GetGroup().GroupNo == gridCells[currentCell.X - 1, currentCell.Y].CurrentPerson.GetGroup().GroupNo)
					neighbourList.Add(gridCells[currentCell.X - 1, currentCell.Y]);

			return neighbourList;
		}

		#endregion

		#endregion

		#region Setup

#if UNITY_EDITOR
		public void Setup(CellInfo[,] cellInfos)
		{
			var width = cellInfos.GetLength(0);
			var height = cellInfos.GetLength(1);
			gridCells = new GridCellMatrix(width, height);

			var xOffset = (nodeSize.x * width + xSpacing * (width - 1)) / 2f - nodeSize.x / 2f;
			var yOffset = (nodeSize.y * height + ySpacing * (height - 1)) / 2f - nodeSize.y / 2f;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					var cellInfo = cellInfos[x, y];
					var cell = (GridCell)PrefabUtility.InstantiatePrefab(cellPrefab, transform);
					cell.Setup(x, y, nodeSize);
					cell.gameObject.name = x + " - " + y;
					var pos = new Vector3(x * (nodeSize.x + xSpacing) - xOffset, 0, -y * (nodeSize.y + ySpacing) + yOffset);
					cell.transform.localPosition = pos;
					gridCells[x, y] = cell;

					if (cellInfo.PersonType != PersonType.None)
					{
						var person = PeopleManager.Instance.SpawnPerson(cellInfo.PersonType, cellInfo.Direction, cellInfo.GroupNo, x, y, pos);
						cell.CurrentPerson = person;
					}

					if (cellInfo.Obstacle is not null)
					{
						var obstacle = ObstacleManager.Instance.SpawnObstacle(cellInfo.Obstacle, x, y, pos);
						cell.CurrentObstacle = obstacle;
						cell.DisableModel();
					}
				}
			}

			PeopleManager.Instance.SetupGroups();
		}
#endif

		[System.Serializable]
		public class GridCellArray
		{
			public GridCell[] Cells;
			public GridCell this[int index]
			{
				get => Cells[index];
				set => Cells[index] = value;
			}

			public GridCellArray(int index0)
			{
				Cells = new GridCell[index0];
			}
		}

		[System.Serializable]
		public class GridCellMatrix
		{
			public GridCellArray[] Arrays;
			public GridCell this[int x, int y]
			{
				get => Arrays[x][y];
				set => Arrays[x][y] = value;
			}

			public GridCellMatrix(int index0, int index1)
			{
				Arrays = new GridCellArray[index0];
				for (int i = 0; i < index0; i++)
					Arrays[i] = new GridCellArray(index1);
			}

			public int GetLength(int dimension)
			{
				return dimension switch
				{
					0 => Arrays.Length,
					1 => Arrays[0].Cells.Length,
					_ => 0
				};
			}
		}

		#endregion

		public GridCell TryToGetCell(int x, int y)
		{
			if (x >= 0 && x < GridCells.GetLength(0) && y >= 0 && y < GridCells.GetLength(1))
				return gridCells[x, y];

			return null;
		}

		public GridCell TryToGetCell(Vector2Int coordinates)
		{
			return TryToGetCell(coordinates.x, coordinates.y);
		}

		public Vector3 GetCellPosition(int x, int y)
		{
			return gridCells[x, y].transform.position;
		}

		public Vector3 GetCellPosition(Vector2Int coordinates)
		{
			return GetCellPosition(coordinates.x, coordinates.y);
		}
	}
}