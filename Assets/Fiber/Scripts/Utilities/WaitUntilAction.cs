using UnityEngine;
using UnityEngine.Events;

namespace Utilities
{
	/// <summary>
	///   <para>Suspends the coroutine execution until the supplied action triggered.</para>
	/// </summary>
	public sealed class WaitUntilAction : CustomYieldInstruction
	{
		private bool actionTriggered = false;

		public override bool keepWaiting => !actionTriggered;

		public WaitUntilAction(ref UnityAction action)
		{
			action -= WaitAction;
			action += WaitAction;
		}

		private void WaitAction()
		{
			actionTriggered = true;
		}
	}
}