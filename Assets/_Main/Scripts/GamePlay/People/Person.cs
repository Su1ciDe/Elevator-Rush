using System.Collections.Generic;
using DG.Tweening;
using Fiber.Utilities.Extensions;
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

		public Tween MoveToSlot(Vector3[] path, SlotHolder slotHolder)
		{
			IsMoving = true;
			animations.Run();

			RemoveFromCell(CurrentCell);

			var seq = DOTween.Sequence();
			if (path is not null)
			{
				var duration = CalculateMovementDuration(transform.position, path[^1]);
				seq.Append(transform.DOPath(path, duration).SetEase(Ease.Linear));
			}

			slotHolder.MoveToSlot(this, ref seq);

			seq.AppendCallback(() =>
			{
				animations.StopRunning();
				IsMoving = false;
			});
			return seq;
		}

		public Tween MoveTo(Vector3 position)
		{
			IsMoving = true;
			// animations.Run();

			var duration = CalculateMovementDuration(transform.position, position);
			transform.DOLookAt(position, .15f).SetEase(Ease.InOutSine);
			return transform.DOMove(position, duration).SetEase(Ease.Linear).OnComplete(() =>
			{
				// animations.StopRunning();
				IsMoving = false;
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