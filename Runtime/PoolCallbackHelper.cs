using System.Collections.Generic;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	internal static class PoolCallbackHelper
	{
		private static readonly List<IPoolable> ComponentsBuffer = new();

		public static void InvokeOnSpawn(GameObject gameObject)
		{
			gameObject.GetComponentsInChildren(ComponentsBuffer);
			foreach (var receiver in ComponentsBuffer)
			{
				receiver.OnSpawn();
			}
		}

		public static void InvokeOnDespawn(GameObject gameObject)
		{
			gameObject.GetComponentsInChildren(ComponentsBuffer);
			foreach (var receiver in ComponentsBuffer)
			{
				receiver.OnDespawn();
			}
		}
	}
}
