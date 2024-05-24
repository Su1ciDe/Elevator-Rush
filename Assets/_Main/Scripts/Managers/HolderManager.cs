using System.Collections.Generic;
using GamePlay;
using TriInspector;
using UnityEngine;

namespace Managers
{
	public class HolderManager : MonoBehaviour
	{
		[Title("Properties")]
		[SerializeField] private int amount;

		[Space]
		[SerializeField] private Holder holderPrefab;

		private List<Holder> holders = new List<Holder>();

		private const float SIZE = 1;

		private void Awake()
		{
			Init();
		}

		private void Init()
		{
			var offset = amount * SIZE / 2f - SIZE / 2f;
			for (int i = 0; i < amount; i++)
			{
				var holder = Instantiate(holderPrefab, transform);
				holder.transform.localPosition = new Vector3(i * SIZE - offset, 0, 0);
				holders.Add(holder);
			}
		}

		public void RearrangePeople()
		{
		}

		public Holder GetFirstEmptyHolder()
		{
			for (int i = 0; i < holders.Count; i++)
			{
				if (!holders[i].CurrentPersonGroup)
					return holders[i];
			}

			return null;
		}

		#region Setup

#if UNITY_EDITOR
		public void Setup(int _amount)
		{
			amount = _amount;
		}
#endif

		#endregion
	}
}