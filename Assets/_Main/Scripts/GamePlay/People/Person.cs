using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Fiber.Managers;
using Fiber.Utilities.Extensions;
using GamePlay.Elevator;
using GridSystem;
using Interfaces;
using LevelEditor;
using Model;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using Utilities;
using Grid = GridSystem.Grid;

namespace GamePlay.People
{
	public class Person : MonoBehaviour, INode
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[field: SerializeField, ReadOnly] public PersonType PersonType { get; private set; }
		[field: SerializeField, ReadOnly] public Direction Direction { get; private set; }

		public bool IsMoving { get; set; }

		[SerializeField] private PersonAnimationController animations;
		public PersonAnimationController Animations => animations;

		public GridCell CurrentCell => Grid.Instance.GridCells[Coordinates.x, Coordinates.y];

		[Space]
		[SerializeField] private float moveSpeed = 10;

		[Title("References")]
		[SerializeField] private SkinnedMeshRenderer meshRenderer;
		[SerializeField] private Collider col;

		private const float HIGHLIGHT_DURATION = .25F;

		public event UnityAction OnTap;
		public event UnityAction OnDown;
		public event UnityAction OnUp;

		private void Awake()
		{
			animations = GetComponentInChildren<PersonAnimationController>();
		}

		private void Start()
		{
			animations.SetRandomIdleSpeed();
		}

		public List<GridCell> CheckPath()
		{
			var xList = new List<int> { Coordinates.x };
			var width = Grid.Instance.GridCells.GetLength(0);
			for (int i = 0; i < width; i++)
			{
				var left = Coordinates.x - 1 * (i + 1);
				if (left >= 0 && left < width && !Grid.Instance.GridCells[left, 0].CurrentObstacle && !Grid.Instance.GridCells[left, 0].CurrentPerson)
					xList.AddIfNotContains(left);

				var right = Coordinates.x + 1 * (i + 1);
				if (right >= 0 && right < width && !Grid.Instance.GridCells[right, 0].CurrentObstacle && !Grid.Instance.GridCells[right, 0].CurrentPerson)
					xList.AddIfNotContains(right);
			}

			List<GridCell> shortestPath = null;
			int shortestPathCount = int.MaxValue;
			for (int i = 0; i < xList.Count; i++)
			{
				var path = Grid.Instance.FindPath(Coordinates.x, Coordinates.y, xList[i], 0);
				if (path is not null && path.Count > 0 && path.Count < shortestPathCount)
				{
					shortestPath = path;
					shortestPathCount = path.Count;
				}
			}

			return shortestPath;
		}

		public List<Vector3> PathList { get; private set; }

		public Tween MoveToSlot(List<Vector3> path, PersonSlotController personSlotController)
		{
			IsMoving = true;
			animations.Run();

			RemoveFromCell(CurrentCell);

			PathList = path is not null ? new List<Vector3>(path) : new List<Vector3>();

			var slot = personSlotController.MoveToSlot(this);

			var tempPath = new List<Vector3>(PathList);
			tempPath.RemoveAt(0);
			var follower = new GameObject { transform = { position = tempPath[0] } };
			follower.transform.DOPath(tempPath.ToArray(), moveSpeed).SetEase(Ease.Linear).SetSpeedBased(true);

			return transform.DOPath(PathList?.ToArray(), moveSpeed).SetEase(Ease.Linear).SetSpeedBased(true).OnUpdate(() => transform.LookAt(follower.transform)).OnComplete(() =>
			{
				transform.SetParent(slot.transform);
				transform.DORotate(slot.transform.eulerAngles, .15f).SetEase(Ease.InOutSine);

				if (personSlotController is Elevator.Elevator)
				{
					LevelManager.Instance.CurrentLevel.ElevatorManager.CalculateValue();
				}

				animations.StopRunning();
				IsMoving = false;
				PathList.Clear();

				Destroy(follower);
			});
		}

		private void RemoveFromCell(GridCell currentCell)
		{
			col.enabled = false;
			currentCell.CurrentPerson = null;
		}

		public void OnMouseDown()
		{
			OnDown?.Invoke();
		}

		public void OnTapped()
		{
			OnTap?.Invoke();
		}

		public void OnMouseUp()
		{
			OnUp?.Invoke();
		}

		public void ShowHighlight()
		{
			transform.DOKill();
			transform.DOScale(1.25f, HIGHLIGHT_DURATION).SetEase(Ease.OutSine);
		}

		public void HideHighlight()
		{
			if (transform.localScale.x.Equals(1)) return;

			transform.DOKill();
			transform.DOScale(1, HIGHLIGHT_DURATION).SetEase(Ease.OutSine);
		}

		private float CalculateMovementDuration(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b) / moveSpeed;
		}

		public PersonGroup GetGroup()
		{
			return GetComponentInParent<PersonGroup>();
		}

#if UNITY_EDITOR
		public void Setup(PersonType type, Material material, Direction direction, Vector2Int coordinates, Vector3 position)
		{
			Direction = direction;
			PersonType = type;
			meshRenderer.material = material;
			Coordinates = coordinates;
			transform.position = position;
			transform.eulerAngles = new Vector3(0, (int)direction * 90);
		}

#endif
	}
}