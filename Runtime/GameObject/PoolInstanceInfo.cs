using UnityEngine;

namespace com.ktgame.manager.pool
{
	[DisallowMultipleComponent, AddComponentMenu("")]
	public sealed class PoolInstanceInfo : MonoBehaviour
	{
		public int SpawnVersion { get; internal set; }
		
		public bool IsInPool { get; internal set; }

		internal object Owner { get; set; }

		internal static PoolInstanceInfo GetOrAdd(GameObject instance, object owner)
		{
			if (!instance.TryGetComponent(out PoolInstanceInfo info))
			{
				info = instance.AddComponent<PoolInstanceInfo>();
				info.hideFlags = HideFlags.HideInInspector;
			}

			info.Owner = owner;
			return info;
		}
		
		public static int GetSpawnVersion(GameObject instance)
		{
			return instance != null && instance.TryGetComponent(out PoolInstanceInfo info) ? info.SpawnVersion : -1;
		}
	}
}
