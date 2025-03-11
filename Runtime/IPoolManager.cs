using com.ktgame.core.manager;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	public interface IPoolManager : IManager
	{
		GameObject Spawn(GameObject prefab);

		GameObject Spawn(GameObject prefab, Transform parent);

		GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation);

		GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent);

		TComponent Spawn<TComponent>(TComponent component) where TComponent : Component;

		TComponent Spawn<TComponent>(TComponent component, Vector3 position, Quaternion rotation, Transform parent) where TComponent : Component;

		TComponent Spawn<TComponent>(TComponent component, Vector3 position, Quaternion rotation) where TComponent : Component;

		TComponent Spawn<TComponent>(TComponent component, Transform parent) where TComponent : Component;

		void Despawn(GameObject instance);

		void Prewarm(GameObject prefab, int count);
	}
}
