using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.People;
using UnityEngine;

namespace GamePlay.Player
{
	public class PlayerInput : MonoBehaviour
	{
		public bool CanInput { get; set; }

		[SerializeField] private LayerMask inputLayer;

		private Person selectedPerson;

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelWin += OnLevelWon;
			LevelManager.OnLevelLose += OnLevelLost;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelWin -= OnLevelWon;
			LevelManager.OnLevelLose -= OnLevelLost;
		}

		private void Update()
		{
			if (!CanInput) return;

			if (Input.GetMouseButtonDown(0))
			{
				OnDown();
			}

			if (Input.GetMouseButton(0))
			{
				OnDrag();
			}

			if (Input.GetMouseButtonUp(0))
			{
				OnUp();
			}
		}

		private void OnDown()
		{
			var person = GetPerson();
			if (!person) return;

			selectedPerson = person;
			selectedPerson.OnMouseDown();
		}

		private void OnDrag()
		{
			var person = GetPerson();
			if (!person)
			{
				if (selectedPerson)
				{
					selectedPerson.OnMouseUp();
					selectedPerson = null;
				}
				return;
			}

			if (!selectedPerson && person.Equals(selectedPerson)) return;

			selectedPerson?.OnMouseUp();
			selectedPerson = person;
			selectedPerson.OnMouseDown();
		}

		private void OnUp()
		{
			if (!selectedPerson) return;

			selectedPerson.OnTapped();
		}

		private Person GetPerson()
		{
			var ray = Helper.MainCamera.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out var hit, 100, inputLayer)) return null;
			return hit.collider.TryGetComponent(out Person person) ? person : null;
		}

		private void OnLevelStarted()
		{
			CanInput = true;
		}

		private void OnLevelLost()
		{
			CanInput = false;
			selectedPerson = null;
		}

		private void OnLevelWon()
		{
			CanInput = false;
			selectedPerson = null;
		}
	}
}