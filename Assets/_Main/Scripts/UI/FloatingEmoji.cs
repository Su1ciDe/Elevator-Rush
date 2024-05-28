using DG.Tweening;
using Fiber.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class FloatingEmoji : MonoBehaviour
	{
		[SerializeField] private float height = 1;
		[SerializeField] private float duration = 1;
		[SerializeField] private Image image;

		private string tagName;

		private void OnDestroy()
		{
			transform.DOComplete();
			image.DOComplete();
			image.transform.DOComplete();
		}

		private void OnEnable()
		{
			var color = image.color;
			color.a = 1;
			image.color = color;
		}

		public void Float(string _tagName)
		{
			tagName = _tagName;
			transform.DOShakeRotation(.2f, 30 * transform.forward, 10, 90, true, ShakeRandomnessMode.Harmonic);
			image.transform.DOPunchScale(.25f * Vector3.one, .2f).OnComplete(() =>
			{
				image.DOFade(0, duration).SetEase(Ease.InExpo);
				transform.DOMoveY(height, duration).SetEase(Ease.InExpo).SetRelative(true).OnComplete(() => ObjectPooler.Instance.Release(gameObject, tagName));
			});
		}
	}
}