using System.Collections;
using Fiber.UI;
using Fiber.Managers;
using Fiber.Utilities;
using Fiber.LevelSystem;
using GamePlay.People;
using GamePlay.Player;
using GamePlay.Elevator;
using TMPro;
using UI;
using UnityEngine;
using Utilities;

namespace Managers
{
	public class TutorialManager : MonoBehaviour
	{
		private TutorialUI tutorialUI => TutorialUI.Instance;
		private Level currentLevel => LevelManager.Instance.CurrentLevel;

		private void Awake()
		{
			LevelManager.OnLevelStart += OnLevelStarted;
			LevelManager.OnLevelUnload += OnLevelUnloaded;
		}

		private void OnDestroy()
		{
			LevelManager.OnLevelStart -= OnLevelStarted;
			LevelManager.OnLevelUnload -= OnLevelUnloaded;

			Unsub();
		}

		private void OnLevelStarted()
		{
			if (LevelManager.Instance.LevelNo.Equals(1))
			{
				Level1Tutorial();
			}
			else if (LevelManager.Instance.LevelNo.Equals(2))
			{
				Level2Tutorial();
			}
		}

		private void OnLevelUnloaded()
		{
			Unsub();

			tutorialUI.HideHand();
			tutorialUI.HideText();
			tutorialUI.HideFocus();
		}

		private void Unsub()
		{
			if (level1_firstGroup)
				level1_firstGroup.OnTapped -= Level1OnTappedOnFirstGroup;
			if (level1_secondGroup)
				level1_secondGroup.OnTapped -= Level1OnTapedOnSecondGroup;

			if (level2_firstGroup)
				level2_firstGroup.OnTapped -= Level2OnTappedOnFirstGroup;
			if (level2_secondGroup)
				level2_secondGroup.OnTapped -= Level2OnTappedOnSecondGroup;

			ElevatorManager.OnNewElevator -= Level1OnNewElevator;
			PeopleManager.OnMovementCompleted -= Level2OnMovementCompleted;
		}

		#region Level1

		private PersonGroup level1_firstGroup;
		private PersonGroup level1_secondGroup;

		private void Level1Tutorial()
		{
			StartCoroutine(DisableInput());
			StartCoroutine(Level1WaitLoading());
		}

		private IEnumerator Level1WaitLoading()
		{
			if (LoadingPanelController.Instance && !LoadingPanelController.Instance.IsLoaded) 
				yield return new WaitUntilAction(ref LoadingPanelController.Instance.OnLoadingFinished);

			level1_firstGroup = PeopleManager.Instance.Groups[1];
			level1_secondGroup = PeopleManager.Instance.Groups[2];
			var txt = currentLevel.ElevatorManager.GetComponentInChildren<TextMeshPro>();

			tutorialUI.ShowFocus(txt.transform.position, Helper.MainCamera);
			tutorialUI.ShowText("Reach the goal!");

			StartCoroutine(Level1Wait());
		}

		private IEnumerator Level1Wait()
		{
			yield return new WaitForSeconds(2.5f);

			tutorialUI.HideFocus();
			tutorialUI.HideText();

			Level1OnTapOnFirstGroup();
		}

		private void Level1OnTapOnFirstGroup()
		{
			Player.Instance.PlayerInput.CanInput = true;

			tutorialUI.ShowTap(level1_firstGroup.People[0].transform.position + 3 * Vector3.up, Helper.MainCamera);
			level1_firstGroup.OnTapped += Level1OnTappedOnFirstGroup;
		}

		private void Level1OnTappedOnFirstGroup()
		{
			level1_firstGroup.OnTapped -= Level1OnTappedOnFirstGroup;

			tutorialUI.HideFocus();
			tutorialUI.HideHand();
			tutorialUI.HideText();

			ElevatorManager.OnNewElevator += Level1OnNewElevator;
		}

		private void Level1OnNewElevator(Elevator elevator)
		{
			ElevatorManager.OnNewElevator -= Level1OnNewElevator;

			if (level1_secondGroup.People[0].CurrentCell.CurrentPerson == level1_secondGroup.People[0])
			{
				tutorialUI.ShowTap(level1_secondGroup.People[0].transform.position + 3 * Vector3.up, Helper.MainCamera);
				level1_secondGroup.OnTapped += Level1OnTapedOnSecondGroup;
			}
		}

		private void Level1OnTapedOnSecondGroup()
		{
			level1_secondGroup.OnTapped -= Level1OnTapedOnSecondGroup;
			tutorialUI.HideHand();

			level1_firstGroup = null;
			level1_secondGroup = null;
		}

		#endregion

		#region Level2

		private PersonGroup level2_firstGroup;
		private PersonGroup level2_secondGroup;

		private void Level2Tutorial()
		{
			StartCoroutine(DisableInput());
			StartCoroutine(Level2WaitLoading());
		}

		private IEnumerator Level2WaitLoading()
		{
			if (LoadingPanelController.Instance && !LoadingPanelController.Instance.IsLoaded)
				yield return new WaitUntilAction(ref LoadingPanelController.Instance.OnLoadingFinished);

			Player.Instance.PlayerInput.CanInput = true;

			level2_firstGroup = PeopleManager.Instance.Groups[2];
			level2_secondGroup = PeopleManager.Instance.Groups[1];

			tutorialUI.ShowText("Clear the way first!");
			tutorialUI.ShowTap(level2_firstGroup.People[0].transform.position + 3 * Vector3.up, Helper.MainCamera);

			level2_firstGroup.OnTapped += Level2OnTappedOnFirstGroup;
		}

		private void Level2OnTappedOnFirstGroup()
		{
			level2_firstGroup.OnTapped -= Level2OnTappedOnFirstGroup;

			tutorialUI.HideText();
			tutorialUI.HideHand();

			Player.Instance.PlayerInput.CanInput = false;

			PeopleManager.OnMovementCompleted += Level2OnMovementCompleted;
		}

		private void Level2OnMovementCompleted()
		{
			PeopleManager.OnMovementCompleted -= Level2OnMovementCompleted;

			Player.Instance.PlayerInput.CanInput = true;

			tutorialUI.ShowText("The way is clear now!");
			tutorialUI.ShowTap(level2_secondGroup.People[0].transform.position + 3 * Vector3.up, Helper.MainCamera);

			level2_secondGroup.OnTapped += Level2OnTappedOnSecondGroup;
		}

		private void Level2OnTappedOnSecondGroup()
		{
			level2_secondGroup.OnTapped -= Level2OnTappedOnSecondGroup;

			tutorialUI.HideText();
			tutorialUI.HideHand();

			level2_firstGroup = null;
			level2_secondGroup = null;
		}

		#endregion

		private IEnumerator DisableInput()
		{
			Player.Instance.PlayerInput.CanInput = false;

			yield return new WaitUntil(() => Player.Instance.PlayerInput.CanInput);
			Player.Instance.PlayerInput.CanInput = false;
		}
	}
}