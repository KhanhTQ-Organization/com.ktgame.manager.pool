using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;

namespace com.ktgame.manager.pool
{
	public sealed class AddressableGameObjectPool : IPool<GameObject>
	{
		public int Count => _stack.Count;

		public bool IsDisposed => _isDisposed;

		public PoolContainer Container { set; private get; }

		private readonly object _key;
		private readonly Stack<GameObject> _stack;
		private TransformSnapshot _template;
		private bool _isDisposed;

		public AddressableGameObjectPool(object key, int capacity = 32)
		{
			_key = key ?? throw new ArgumentNullException(nameof(key));
			_stack = new Stack<GameObject>(Mathf.Max(0, capacity));
		}

		public AddressableGameObjectPool(AssetReferenceGameObject reference, int capacity = 32)
		{
			if (reference == null)
			{
				throw new ArgumentNullException(nameof(reference));
			}

			_key = reference.RuntimeKey;
			_stack = new Stack<GameObject>(Mathf.Max(0, capacity));
		}
		
		public bool Contains(GameObject instance)
		{
			return instance != null &&
			       instance.TryGetComponent(out PoolInstanceInfo info) &&
			       info.IsInPool &&
			       ReferenceEquals(info.Owner, this);
		}

		public GameObject Spawn()
		{
			ThrowIfDisposed();

			var obj = PopAlive();
			if (obj == null)
			{
				obj = Create(AddressablePoolUtility.Instantiate(Addressables.InstantiateAsync(_key), _key), true);
			}
			else
			{
				obj.transform.SetParent(null, false);
				_template?.ApplyTo(obj.transform);
				obj.SetActive(true);
			}

			return OnSpawned(obj);
		}

		public GameObject Spawn(Transform parent)
		{
			ThrowIfDisposed();

			var obj = PopAlive();
			if (obj == null)
			{
				obj = Create(AddressablePoolUtility.Instantiate(Addressables.InstantiateAsync(_key, parent), _key), true);
			}
			else
			{
				obj.transform.SetParent(parent, false);
				_template?.ApplyTo(obj.transform);
				obj.SetActive(true);
			}

			return OnSpawned(obj);
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation)
		{
			ThrowIfDisposed();

			var obj = PopAlive();
			if (obj == null)
			{
				obj = Create(AddressablePoolUtility.Instantiate(Addressables.InstantiateAsync(_key, position, rotation), _key), false);
			}
			else
			{
				obj.transform.SetParent(null, false);
				_template?.ApplyTo(obj.transform);
				obj.transform.SetPositionAndRotation(position, rotation);
				obj.SetActive(true);
			}

			return OnSpawned(obj);
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent)
		{
			ThrowIfDisposed();

			var obj = PopAlive();
			if (obj == null)
			{
				obj = Create(AddressablePoolUtility.Instantiate(Addressables.InstantiateAsync(_key, position, rotation, parent), _key), false);
			}
			else
			{
				obj.transform.SetParent(parent, false);
				_template?.ApplyTo(obj.transform);
				obj.transform.SetPositionAndRotation(position, rotation);
				obj.SetActive(true);
			}

			return OnSpawned(obj);
		}

		public void Despawn(GameObject instance)
		{
			ThrowIfDisposed();

			if (instance == null) return;

			var info = PoolInstanceInfo.GetOrAdd(instance, this);
			if (info.IsInPool)
			{
				LogWarning($"[Pool] '{instance.name}' is already in the pool - duplicate Despawn ignored.", instance);
				return;
			}

			PoolCallbackHelper.InvokeOnDespawn(instance);

			instance.SetActive(false);
			if (Container != null)
			{
				instance.transform.SetParent(Container.transform, false);
			}

			info.IsInPool = true;
			_stack.Push(instance);
		}

		public void Prewarm(int count)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var obj = Create(AddressablePoolUtility.Instantiate(Addressables.InstantiateAsync(_key), _key), true);
				var info = PoolInstanceInfo.GetOrAdd(obj, this);
				PoolCallbackHelper.InvokeOnDespawn(obj);
				obj.SetActive(false);
				if (Container != null)
				{
					obj.transform.SetParent(Container.transform, false);
				}

				info.IsInPool = true;
				_stack.Push(obj);
			}
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				AddressablePoolUtility.ReleaseInstance(obj);
			}
		}

		public void Dispose()
		{
			if (_isDisposed)
			{
				return;
			}

			Clear();
			_isDisposed = true;
		}
		
		private GameObject Create(GameObject obj, bool captureTemplate)
		{
			if (captureTemplate && _template == null)
			{
				_template = new TransformSnapshot(obj.transform);
			}

			return obj;
		}

		private GameObject PopAlive()
		{
			while (_stack.TryPop(out var obj))
			{
				if (obj != null)
				{
					return obj;
				}
			}

			return null;
		}

		private GameObject OnSpawned(GameObject obj)
		{
			var info = PoolInstanceInfo.GetOrAdd(obj, this);
			info.IsInPool = false;
			info.SpawnVersion++;

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT")]
		private static void LogWarning(string message, UnityEngine.Object context)
		{
			Debug.LogWarning(message, context);
		}

		[HideInCallstack, Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT")]
		private void ThrowIfDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}
	}
}
