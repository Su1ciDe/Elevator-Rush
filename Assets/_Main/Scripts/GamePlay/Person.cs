using DG.Tweening;
using Interfaces;
using TriInspector;
using UnityEngine;
using Utilities;

namespace GamePlay
{
	public class Person : MonoBehaviour, INode
	{
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }

		[field: SerializeField, ReadOnly] public PersonType PersonType { get; private set; }

		[Space]
		[SerializeField] private MeshRenderer meshRenderer;

		private const float HIGHLIGHT_DURATION = .25F;

		public void Setup(PersonType type, Material material, Vector3 position)
		{
			PersonType = type;
			meshRenderer.material = material;
		}

		public void CheckIfCanMove()
		{
			HideHighlight();
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