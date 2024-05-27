using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Fiber.Utilities;
using Fiber.LevelSystem;
using GamePlay;
using GamePlay.Obstacles;
using ScriptableObjects;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Utilities;

namespace LevelEditor.Editor
{
	public class LevelEditor : EditorWindow
	{
		private static LevelEditor window;

		#region Elements

		[SerializeField] private VisualTreeAsset treeAsset;

		// Load
		private VisualElement Load_VE;
		private ObjectField levelField;
		private Button btn_Load;

		// Grid
		private VisualElement Grid_VE;
		private Vector2IntField v2Field_Size;
		private Button btn_SetupGrid;
		private Label lbl_Direction;

		private RadioButtonGroup radio_PeopleObstacle;
		private UnsignedIntegerField txt_GroupNo;
		private EnumField EnumField_PeopleType;
		private DropdownField drop_Obstacle;

		// Options
		private VisualElement Elevators_VE;
		private ListView listView_Elevator;
		private SliderInt slider_HolderCount;
		private UnsignedIntegerField txt_LevelNo;
		private Button btn_Save;

		#endregion

		private List<ElevatorData> elevators = new List<ElevatorData>();

		private PersonDataSO personDataSO;
		private List<BaseObstacle> obstacles;
		private Level loadedLevel;

		private Direction direction = Direction.Up;
		private const string DIRECTION_LABEL_TEXT = "lbl_Direction";

		#region Paths

		private const string LEVELS_PATH = "Assets/_Main/Prefabs/Levels/";
		private static readonly string LEVEL_BASE_PREFAB_PATH = $"{LEVELS_PATH}_BaseLevel.prefab";
		private const string PERSON_DATA_PATH = "Assets/_Main/ScriptableObjects/PersonData.asset";
		private const string OBSTACLES_PATH = "Assets/_Main/Prefabs/Obstacles/";

		#endregion

		private static Vector2Int maxSize = new Vector2Int(12, 12);

		[MenuItem("Elevator Rush/Level Editor")]
		private static void ShowWindow()
		{
			window = GetWindow<LevelEditor>();
			window.titleContent = new GUIContent("Level Editor");
			window.minSize = new Vector2(750, 750);
			window.Show();
		}

		private void CreateGUI()
		{
			InitElements();

			personDataSO = AssetDatabase.LoadAssetAtPath<PersonDataSO>(PERSON_DATA_PATH);
		}

		private void InitElements()
		{
			treeAsset.CloneTree(rootVisualElement);

			// Load
			Load_VE = rootVisualElement.Q<VisualElement>(nameof(Load_VE));
			levelField = EditorUtilities.CreateVisualElement<ObjectField>();
			levelField.label = "Load Level";
			levelField.style.flexGrow = 1;
			levelField.objectType = typeof(Level);
			Load_VE.Add(levelField);
			btn_Load = rootVisualElement.Q<Button>(nameof(btn_Load));
			btn_Load.clickable.clicked += Load;

			//Grid
			Grid_VE = rootVisualElement.Q<VisualElement>(nameof(Grid_VE));
			v2Field_Size = rootVisualElement.Q<Vector2IntField>(nameof(v2Field_Size));
			v2Field_Size.RegisterValueChangedCallback(evt => v2Field_Size.value = new Vector2Int(Mathf.Clamp(evt.newValue.x, 0, maxSize.x), Mathf.Clamp(evt.newValue.y, 0, maxSize.y)));
			btn_SetupGrid = rootVisualElement.Q<Button>(nameof(btn_SetupGrid));
			btn_SetupGrid.clickable.clicked += SetupGrid;
			drop_Obstacle = rootVisualElement.Q<DropdownField>(nameof(drop_Obstacle));
			drop_Obstacle.choices = LoadObstacles();
			txt_GroupNo = rootVisualElement.Q<UnsignedIntegerField>(nameof(txt_GroupNo));
			EnumField_PeopleType = rootVisualElement.Q<EnumField>(nameof(EnumField_PeopleType));
			EnumField_PeopleType.RegisterValueChangedCallback(evt => OnPeopleTypeChanged((PersonType)evt.newValue));
			radio_PeopleObstacle = rootVisualElement.Q<RadioButtonGroup>(nameof(radio_PeopleObstacle));
			radio_PeopleObstacle.RegisterValueChangedCallback(evt =>
			{
				if (evt.newValue.Equals(0))
				{
					EnumField_PeopleType.SetEnabled(true);
					drop_Obstacle.SetEnabled(false);
				}
				else if (evt.newValue.Equals(1))
				{
					EnumField_PeopleType.SetEnabled(false);
					drop_Obstacle.SetEnabled(true);
				}
			});
			lbl_Direction = rootVisualElement.Q<Label>(nameof(lbl_Direction));

			// Options
			Elevators_VE = rootVisualElement.Q<VisualElement>(nameof(Elevators_VE));
			SetupElevators();
			Elevators_VE.Add(listView_Elevator);

			slider_HolderCount = rootVisualElement.Q<SliderInt>(nameof(slider_HolderCount));
			slider_HolderCount.value = 5;
			txt_LevelNo = rootVisualElement.Q<UnsignedIntegerField>(nameof(txt_LevelNo));
			btn_Save = rootVisualElement.Q<Button>(nameof(btn_Save));
			btn_Save.clickable.clicked += Save;
		}

		private List<string> LoadObstacles()
		{
			var obstaclePrefabs = EditorUtilities.LoadAllAssetsFromPath<BaseObstacle>(OBSTACLES_PATH);
			obstacles = obstaclePrefabs.Where(x => !x.name.Contains("_BaseObstacle")).ToList();
			return obstacles.Select(x => x.name).ToList();
		}

		private void SetupElevators()
		{
			listView_Elevator = new ListView(elevators)
			{
				headerTitle = "Elevators",
				showFoldoutHeader = true,
				virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
				reorderable = false,
				showAddRemoveFooter = true,
				makeItem = () =>
				{
					var elevatorDataVisualElement = new ElevatorDataVisualElement();

					var enumValue = elevatorDataVisualElement.Q<EnumField>("enum_Value");
					int i = 0;
					if (enumValue.userData is not null)
					{
						i = (int)enumValue.userData;
						enumValue.value = elevators[i].Value;
					}

					enumValue.RegisterValueChangedCallback(evt => elevators[i].Value = (ElevatorValueType)evt.newValue);

					var enumColor = elevatorDataVisualElement.Q<EnumField>("enum_Color");
					int j = 0;
					if (enumColor.userData is not null)
					{
						j = (int)enumColor.userData;
						enumColor.value = elevators[j].ElevatorType;
					}

					enumColor.RegisterValueChangedCallback(evt => elevators[j].ElevatorType = (PersonType)evt.newValue);
					return elevatorDataVisualElement;
				},
				bindItem = (e, i) =>
				{
					elevators[i] ??= new ElevatorData();

					var enumColor = e.Q<EnumField>("enum_Color");
					var enumValue = e.Q<EnumField>("enum_Value");
					enumValue.userData = i;
					enumColor.userData = i;
					enumValue.value = elevators[i].Value;
					enumColor.value = elevators[i].ElevatorType;

					e.RegisterCallback<ChangeEvent<ElevatorData>>(value => elevators[i] = value.newValue);
				},
			};
		}

		#region Grid

		private CellInfo[,] gridCells;

		private void SetupGrid()
		{
			Grid_VE.Clear();
			gridCells = new CellInfo[v2Field_Size.value.x, v2Field_Size.value.y];

			for (int y = 0; y < gridCells.GetLength(1); y++)
			{
				var row = EditorUtilities.CreateVisualElement<VisualElement>("gridRow");
				Grid_VE.Add(row);
				for (int x = 0; x < gridCells.GetLength(0); x++)
				{
					gridCells[x, y] = new CellInfo();
					var button = EditorUtilities.CreateVisualElement<Button>("cell");
					button.focusable = false;
					gridCells[x, y].Coordinates = new Vector2Int(x, y);
					gridCells[x, y].Color = Color.white;
					gridCells[x, y].Button = button;

					int x1 = x;
					int y1 = y;

					button.RegisterCallback<PointerDownEvent>(e => Delete(e.clickCount, gridCells[x1, y1]), TrickleDown.TrickleDown);
					button.RegisterCallback<MouseDownEvent>(e => OnCellClicked(e, gridCells[x1, y1]), TrickleDown.TrickleDown);

					row.Add(button);
				}
			}
		}

		private void OnCellClicked(MouseDownEvent e, CellInfo cellInfo)
		{
			if (e.button.Equals(0)) // Place
			{
				if (radio_PeopleObstacle.value.Equals(0))
				{
					if (selectedType == PersonType.None)
					{
						cellInfo.Button.style.backgroundColor = Color.white;
						cellInfo.Obstacle = null;
						cellInfo.PersonType = PersonType.None;
					}
					else
					{
						cellInfo.Button.style.backgroundColor = personDataSO.PersonData[selectedType].color;
						cellInfo.PersonType = selectedType;
						cellInfo.GroupNo = (int)txt_GroupNo.value;
						cellInfo.Button.text = cellInfo.GroupNo.ToString();
						cellInfo.Direction = direction;

						SetLabelDirection(direction, cellInfo.Button);
					}
				}
				else if (radio_PeopleObstacle.value.Equals(1))
				{
					cellInfo.Button.style.backgroundColor = Color.black;
					cellInfo.Button.text = drop_Obstacle.value;
					cellInfo.Obstacle = obstacles[drop_Obstacle.index];
				}
			}
			else if (e.button.Equals(1)) // Rotate
			{
				direction = direction switch
				{
					Direction.Up => Direction.Left,
					Direction.Left => Direction.Down,
					Direction.Down => Direction.Right,
					Direction.Right => Direction.Up,
					_ => direction
				};

				lbl_Direction.text = direction.ToString();
			}
		}

		private void Delete(int clickCount, CellInfo cellInfo)
		{
			if (clickCount <= 1) return;
			EditorCoroutineUtility.StartCoroutine(DeleteCoroutine(), this);
			return;

			IEnumerator DeleteCoroutine()
			{
				yield return null;

				cellInfo.Button.style.backgroundColor = Color.white;
				cellInfo.Obstacle = null;
				cellInfo.PersonType = PersonType.None;
				cellInfo.Button.text = "";

				var lblDirection = cellInfo.Button.Q<Label>(DIRECTION_LABEL_TEXT);
				if (lblDirection is not null)
					lblDirection.text = "";
			}
		}

		private void SetLabelDirection(Direction dir, VisualElement ve)
		{
			var lblDirection = ve.Q<Label>(DIRECTION_LABEL_TEXT);
			if (lblDirection is null)
			{
				lblDirection = EditorUtilities.CreateVisualElement<Label>();
				lblDirection.name = DIRECTION_LABEL_TEXT;
			}

			lblDirection.text = dir.ToString();
			ve.Add(lblDirection);
		}

		private PersonType selectedType = PersonType.None;

		private void OnPeopleTypeChanged(PersonType type)
		{
			selectedType = type;
		}

		#endregion

		#region Save

		private GameObject levelBasePrefab;

		private void Save()
		{
			var source = AssetDatabase.LoadAssetAtPath<GameObject>(LEVEL_BASE_PREFAB_PATH);
			// Need to instantiate this prefab to the scene in order to create a variant
			levelBasePrefab = (GameObject)PrefabUtility.InstantiatePrefab(source);
			var levelBase = levelBasePrefab.GetComponent<Level>();

			EditorUtility.SetDirty(levelBasePrefab);
			EditorSceneManager.MarkSceneDirty(levelBasePrefab.scene);

			//
			SetupLevel(levelBase);
			//

			var levelPath = $"{LEVELS_PATH}Level_{txt_LevelNo.value:000}.prefab";
			if (loadedLevel)
			{
				AssetDatabase.DeleteAsset(levelPath);
			}

			levelPath = AssetDatabase.GenerateUniqueAssetPath(levelPath);
			PrefabUtility.SaveAsPrefabAsset(levelBasePrefab, levelPath);

			EditorUtility.ClearDirty(levelBasePrefab);

			AssetDatabase.Refresh();
			Debug.Log($"<color=lime>{levelPath} has saved!</color>");

			DestroyImmediate(levelBasePrefab);
			levelBasePrefab = null;
		}

		private void SetupLevel(Level level)
		{
			level.Grid.Setup(gridCells);
			level.ElevatorManager.Setup(elevators);
			level.HolderManager.Setup(slider_HolderCount.value);
		}

		#endregion

		#region Load

		private void Load()
		{
			if (!levelField.value) return;
			loadedLevel = AssetDatabase.LoadAssetAtPath<Level>(AssetDatabase.GetAssetPath(levelField.value.GetInstanceID()));

			txt_LevelNo.value = (uint)ParseLevelNo(loadedLevel.name);
			v2Field_Size.value = new Vector2Int(loadedLevel.Grid.GridCells.GetLength(0), loadedLevel.Grid.GridCells.GetLength(1));

			slider_HolderCount.value = loadedLevel.HolderManager.Amount;

			SetupGrid();

			foreach (var groupPair in loadedLevel.PeopleManager.Groups)
			{
				foreach (var person in groupPair.Value.People)
				{
					var cell = gridCells[person.Coordinates.x, person.Coordinates.y];
					cell.PersonType = person.PersonType;
					cell.GroupNo = groupPair.Key;
					cell.Coordinates = person.Coordinates;
					cell.Color = personDataSO.PersonData[person.PersonType].color;
					cell.Button.style.backgroundColor = cell.Color;
					cell.Button.text = groupPair.Key.ToString();
					cell.Direction = person.Direction;

					SetLabelDirection(cell.Direction, cell.Button);
				}
			}

			obstacles = loadedLevel.ObstacleManager.Obstacles;
			foreach (var obstacle in loadedLevel.ObstacleManager.Obstacles)
			{
				var cell = gridCells[obstacle.Coordinates.x, obstacle.Coordinates.y];
				cell.Obstacle = obstacle;
				cell.Button.style.backgroundColor = cell.Color = Color.black;
				cell.Button.text = obstacle.name;
			}

			var temp = new List<ElevatorData>(loadedLevel.ElevatorManager.Elevators.Select(x => x.ElevatorData));
			elevators = new List<ElevatorData>(temp);

			listView_Elevator.itemsSource = elevators;
			listView_Elevator.Rebuild();
			listView_Elevator.RefreshItems();
		}

		private int ParseLevelNo(string levelName)
		{
			return int.TryParse(levelName.Substring(levelName.Length - 3, 3), out var levelNo) ? levelNo : 0;
		}

		#endregion
	}
}