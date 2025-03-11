using System;
using System.Collections.Generic;
using com.ktgame.core.di;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	public class PoolManager : MonoBehaviour, IPoolManager
	{
		[SerializeField] private List<PoolPreset> _poolPresets;

		public int Priority => 0;

		public bool IsInitialized { get; private set; }

		[Inject] private readonly IInjector _injector;

		[ShowInInspector] private readonly Dictionary<GameObject, GameObjectPool> _pools = new();
		[ShowInInspector] private readonly Dictionary<GameObject, GameObjectPool> _cloneReferences = new();

		public UniTask Initialize()
		{
			_pools.Clear();
			_cloneReferences.Clear();

			if (_poolPresets.Count > 0)
			{
				foreach (var preset in _poolPresets)
				{
					if (preset.Enabled && preset.Prefab != null)
					{
						var pool = GetOrCreatePool(preset.Prefab, preset.Capacity);
						pool.Prewarm(preset.PreloadSize);
					}
				}
			}

			IsInitialized = true;
			return UniTask.CompletedTask;
		}

		public GameObject Spawn(GameObject prefab)
		{
			if (prefab == null)
			{
				throw new ArgumentNullException(nameof(prefab));
			}

			var pool = GetOrCreatePool(prefab);
			var obj = pool.Spawn();
			_injector.Resolve(obj);
			_cloneReferences.Add(obj, pool);
			return obj;
		}

		public GameObject Spawn(GameObject prefab, Transform parent)
		{
			if (prefab == null)
			{
				throw new ArgumentNullException(nameof(prefab));
			}

			var pool = GetOrCreatePool(prefab);
			var obj = pool.Spawn(parent);
			_injector.Resolve(obj);
			_cloneReferences.Add(obj, pool);
			return obj;
		}

		public GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation)
		{
			if (original == null)
			{
				throw new ArgumentNullException(nameof(original));
			}

			var pool = GetOrCreatePool(original);
			var obj = pool.Spawn(position, rotation);
			_injector.Resolve(obj);
			_cloneReferences.Add(obj, pool);
			return obj;
		}

		public GameObject Spawn(GameObject original, Vector3 position, Quaternion rotation, Transform parent)
		{
			if (original == null)
			{
				throw new ArgumentNullException(nameof(original));
			}

			var pool = GetOrCreatePool(original);
			var obj = pool.Spawn(position, rotation, parent);
			_injector.Resolve(obj);
			_cloneReferences.Add(obj, pool);
			return obj;
		}

		public TComponent Spawn<TComponent>(TComponent component) where TComponent : Component
		{
			var instance = Spawn(component.gameObject).GetComponent<TComponent>();
			_injector.Resolve(instance);
			return instance;
		}

		public TComponent Spawn<TComponent>(TComponent component, Vector3 position, Quaternion rotation, Transform parent)
			where TComponent : Component
		{
			var instance = Spawn(component.gameObject, position, rotation, parent).GetComponent<TComponent>();
			_injector.Resolve(instance);
			return instance;
		}

		public TComponent Spawn<TComponent>(TComponent component, Vector3 position, Quaternion rotation) where TComponent : Component
		{
			var instance = Spawn(component.gameObject, position, rotation).GetComponent<TComponent>();
			_injector.Resolve(instance);
			return instance;
		}

		public TComponent Spawn<TComponent>(TComponent component, Transform parent) where TComponent : Component
		{
			var instance = Spawn(component.gameObject, parent).GetComponent<TComponent>();
			_injector.Resolve(instance);
			return instance;
		}

		public void Despawn(GameObject instance)
		{
			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (_cloneReferences.TryGetValue(instance, out var pool))
			{
				_cloneReferences.Remove(instance);
				pool.Despawn(instance);
			}
		}

		public void Prewarm(GameObject prefab, int count)
		{
			if (prefab == null)
			{
				throw new ArgumentNullException(nameof(prefab));
			}

			var pool = GetOrCreatePool(prefab);
			pool.Prewarm(count);
		}

		public void Dispose()
		{
			_pools.Clear();
			_cloneReferences.Clear();
		}

		private GameObjectPool GetOrCreatePool(GameObject prefab, int capacity = 32)
		{
			if (!_pools.TryGetValue(prefab, out var pool))
			{
				var container = new GameObject($"[Pool] - {prefab.name}").AddComponent<PoolContainer>();
				container.transform.SetParent(transform);
				pool = new GameObjectPool(prefab, capacity);
				pool.Container = container;
				_pools.Add(prefab, pool);
			}

			return pool;
		}
	}
}
