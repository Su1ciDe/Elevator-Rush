using System.Collections.Generic;
using System.Linq;
using Fiber.Utilities;
using Fiber.LevelSystem;
using GamePlay.Obstacles;
using ScriptableObjects;
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
		
		private RadioButtonGroup radio_PeopleObstacle;
		private UnsignedIntegerField txt_GroupNo;
		private EnumField EnumField_PeopleType;
		private DropdownField drop_Obstacle;

		// Options
		private UnsignedIntegerField txt_LevelNo;
		private Button btn_Save;

		#endregion

		private PersonDataSO personDataSO;
		private List<BaseObstacle> obstacles;
		private Level loadedLevel;

		#region Paths

		private const string LEVELS_PATH = "Assets/_Main/Prefabs/Levels/";
		private static readonly string LEVEL_BASE_PREFAB_PATH = $"{LEVELS_PATH}_BaseLevel.prefab";
		private const string PERSON_DATA_PATH = "Assets/_Main/ScriptableObjects/PersonData.asset";
		private const string OBSTACLES_PATH = "Assets/_Main/Prefabs/Obstacles/";

		#endregion

		private static Vector2Int maxSize = new Vector2Int(8, 8);

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
			// levelField.RegisterValueChangedCallback(evt => LoadPreset((CharEditorPresetSO)evt.newValue));
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

			// Save
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
					button.RegisterCallback<MouseDownEvent>(e => OnCellClicked(e, gridCells[x1, y1]), TrickleDown.TrickleDown);

					row.Add(button);
				}
			}
		}

		private void OnCellClicked(MouseDownEvent e, CellInfo cellInfo)
		{
			if (e.button.Equals(0))
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

					}
				}
				else if (radio_PeopleObstacle.value.Equals(1))
				{
					cellInfo.Button.style.backgroundColor = Color.black;
					cellInfo.Button.text = drop_Obstacle.value;
					cellInfo.Obstacle = obstacles[drop_Obstacle.index];
				}
			}
			else if (e.button.Equals(1))
			{
				cellInfo.Button.style.backgroundColor = Color.white;
				cellInfo.Obstacle = null;
				cellInfo.PersonType = PersonType.None;
			}
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
			levelPath = AssetDatabase.GenerateUniqueAssetPath(levelPath);
			PrefabUtility.SaveAsPrefabAsset(levelBasePrefab, levelPath);

			EditorUtility.ClearDirty(levelBasePrefab);

			AssetDatabase.Refresh();
			Debug.Log($"{levelPath} has saved!");

			DestroyImmediate(levelBasePrefab);
			levelBasePrefab = null;
		}

		private void SetupLevel(Level level)
		{
			level.Grid.Setup(gridCells);
		}

		#endregion

		#region Load

		private void Load()
		{
		}

		#endregion
	}
}