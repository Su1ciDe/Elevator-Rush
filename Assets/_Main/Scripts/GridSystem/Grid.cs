using Fiber.Managers;
using Fiber.Utilities;
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
					var pos =  new Vector3(x * (nodeSize.x + xSpacing) - xOffset, 0, -y * (nodeSize.y + ySpacing) + yOffset);
					cell.transform.localPosition = pos;
					gridCells[x, y] = cell;

					if (cellInfo.PersonType != PersonType.None)
					{
						PeopleManager.Instance.SpawnPerson(cellInfo.PersonType, cellInfo.GroupNo, x, y,pos);
					}

					if (cellInfo.Obstacle is not null)
					{
						ObstacleManager.Instance.SpawnObstacle(cellInfo.Obstacle);
					}
				}
			}
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

		public Vector3 GetCellPosition(int x, int y)
		{
			return gridCells[x, y].transform.position;
		}
	}
}