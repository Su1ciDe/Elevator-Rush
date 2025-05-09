using Fiber.Utilities;
using UnityEngine;

namespace Fiber.Managers
{
	[DefaultExecutionOrder(-1)]
	public class GameManager : SingletonInit<GameManager>
	{
		protected override void Awake()
		{
			base.Awake();
			Application.targetFrameRate = 60;
			Input.multiTouchEnabled = false;
			Debug.unityLogger.logEnabled = Debug.isDebugBuild;
		}
	}
}