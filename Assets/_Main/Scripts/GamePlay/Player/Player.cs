using Fiber.Managers;
using Fiber.Utilities;
using UnityEngine;

namespace GamePlay.Player
{
	/// <summary>
	/// /// Handle Player Specific Information Assigning,  ...
	/// </summary>
	public class Player : Singleton<Player>
	{
		public PlayerInput PlayerInput { get; private set; }

		private void Awake()
		{
			PlayerInput = GetComponent<PlayerInput>();
		}

		private void Start()
		{
			// TODO
		}

		private void OnLevelLoaded()
		{
		}

		// OnStart is called when click "tap to play button"
		private void OnStart()
		{
			// TODO  
		}

		// OnWin is called when game is completed as succeed
		private void OnWin()
		{
			// TODO
		}

		// OnLose is called when game is completed as failed
		private void OnLose()
		{
			// TODO
		}

		private void OnEnable()
		{
			LevelManager.OnLevelLoad += OnLevelLoaded;
			LevelManager.OnLevelStart += OnStart;
			LevelManager.OnLevelWin += OnWin;
			LevelManager.OnLevelLose += OnLose;
		}

		private void OnDisable()
		{
			LevelManager.OnLevelLoad -= OnLevelLoaded;
			LevelManager.OnLevelStart -= OnStart;
			LevelManager.OnLevelWin -= OnWin;
			LevelManager.OnLevelLose -= OnLose;
		}
	}
}