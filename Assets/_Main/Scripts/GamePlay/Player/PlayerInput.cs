using Fiber.Managers;
using Fiber.Utilities;
using GamePlay.People;
using Lean.Touch;
using Managers;
using UnityEngine;

namespace GamePlay.Player
{
	public class PlayerInput : MonoBehaviour
	{
		public bool CanInput { get; set; } = false;

		[SerializeField] private LayerMask inputLayer;

		private Person selectedPerson;

		private bool isDown;

		private void OnEnable()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelWin += OnLevelWon;
			LevelManager.OnLevelLose += OnLevelLost;

			LeanTouch.OnFingerDown += OnDown;
			LeanTouch.OnFingerUpdate += OnDrag;
			LeanTouch.OnFingerUp += OnUp;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelWin -= OnLevelWon;
			LevelManager.OnLevelLose -= OnLevelLost;

			LeanTouch.OnFingerDown -= OnDown;
			LeanTouch.OnFingerUpdate -= OnDrag;
			LeanTouch.OnFingerUp -= OnUp;
		}

		private void OnDestroy()
		{
			ElevatorManager.OnNewElevator -= OnNewElevator;
		}

		private void OnDown(LeanFinger finger)
		{
			if (!CanInput) return;
			if (finger.IsOverGui) return;

			isDown = true;
			var person = GetPerson(finger);
			if (!person) return;

			selectedPerson = person;
			selectedPerson.OnMouseDown();
		}

		private void OnDrag(LeanFinger finger)
		{
			if (!CanInput) return;
			if (finger.IsOverGui) return;

			if (!isDown) return;
			var person = GetPerson(finger);
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

		private void OnUp(LeanFinger finger)
		{
			if (!CanInput) return;
			if (finger.IsOverGui) return;

			isDown = false;
			if (!selectedPerson) return;

			selectedPerson.OnTapped();
			selectedPerson = null;
		}

		private Person GetPerson(LeanFinger finger)
		{
			var ray = finger.GetRay(Helper.MainCamera);
			// var ray = Helper.MainCamera.ScreenPointToRay(Input.mousePosition);
			if (!Physics.Raycast(ray, out var hit, 100, inputLayer)) return null;
			return hit.collider.TryGetComponent(out Person person) ? person : null;
		}

		private void OnNewElevator(Elevator.Elevator _)
		{
			CanInput = true;
			ElevatorManager.OnNewElevator -= OnNewElevator;
		}

		private void OnLevelStarted()
		{
			ElevatorManager.OnNewElevator += OnNewElevator;
			selectedPerson = null;
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