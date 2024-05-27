using Fiber.Animation;
using UnityEngine;

namespace GamePlay.People
{
	public class PersonAnimationController : AnimationController
	{
		public void ChooseIdle(int groupNumber)
		{
			switch (groupNumber)
			{
				case 0:
					SetBool(AnimationType.idle1, true);
					break;
				case 1:
					SetBool(AnimationType.idle2, true);
					break;
				case 2:
					SetBool(AnimationType.idle3, true);
					break;
				case 3:
					SetBool(AnimationType.idle4, true);
					break;
				case 4:
					SetBool(AnimationType.idle5, true);
					break;
			}
		}

		public void StopIdling()
		{
			SetBool(AnimationType.idle1, false);
			SetBool(AnimationType.idle2, false);
			SetBool(AnimationType.idle3, false);
			SetBool(AnimationType.idle4, false);
			SetBool(AnimationType.idle5, false);
		}

		public void SetRandomIdleSpeed()
		{
			SetFloat(AnimationType.idleSpeed, Random.Range(0.75f, 1.5f));
		}

		public void Run()
		{
			SetBool(AnimationType.Run, true);
		}

		public void StopRunning()
		{
			SetBool(AnimationType.Run, false);
		}
	}
}