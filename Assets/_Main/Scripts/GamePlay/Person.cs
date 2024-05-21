using System.Collections.Generic;
using DG.Tweening;
using Fiber.Utilities.Extensions;
using Interfaces;
using TriInspector;
using UnityEngine;
using Utilities;
using Grid = GridSystem.Grid;

namespace GamePlay
{
	public class Person : MonoBehaviour, INode
	{
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[field: SerializeField, ReadOnly] public PersonType PersonType { get; private set; }

		[Space]
		[SerializeField] private MeshRenderer meshRenderer;

		private const float HIGHLIGHT_DURATION = .25F;

		public void Setup(PersonType type, Material material, Vector2Int coordinates, Vector3 position)
		{
			PersonType = type;
			meshRenderer.material = material;
			Coordinates = coordinates;
			transform.position = position;
		}

		public void CheckIfCanMove()
		{
			HideHighlight();

			var xList = new List<int> { Coordinates.x };
			var width = Grid.Instance.GridCells.GetLength(0);
			for (int i = 0; i < width; i++)
			{
				var left = Coordinates.x + i - 1;
				if (left >= 0 && left < width && !Grid.Instance.GridCells[left, 0].CurrentObstacle && !Grid.Instance.GridCells[left, 0].CurrentPerson)
					xList.AddIfNotContains(left);

				var right = Coordinates.x + i + 1;
				if (right >= 0 && right < width && !Grid.Instance.GridCells[right, 0].CurrentObstacle && !Grid.Instance.GridCells[right, 0].CurrentPerson)
					xList.AddIfNotContains(right);
			}

			for (int i = 0; i < xList.Count; i++)
			{
				var path = Grid.Instance.FindPath(Coordinates.x, Coordinates.y, xList[i], 0);
				if (path is not null && path.Count > 0)
				{
					//

					break;
				}
			}
		}

		public void Highlight()
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
	}
}