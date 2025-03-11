using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.ktgame.manager.pool
{
	public sealed class AddressableGameObjectPool : IPool<GameObject>
	{
		public int Count => _stack.Count;

		public bool IsDisposed => _isDisposed;

		public PoolContainer Container { set; private get; }

		private readonly object _key;
		private readonly Stack<GameObject> _stack;
		private bool _isDisposed;

		public AddressableGameObjectPool(object key, int capacity = 32)
		{
			_key = key;
			_stack = new Stack<GameObject>(capacity);
		}

		public AddressableGameObjectPool(AssetReferenceGameObject reference, int capacity = 32)
		{
			_key = reference.RuntimeKey;
			_stack = new Stack<GameObject>(capacity);
		}

		public GameObject Spawn()
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = Addressables.InstantiateAsync(_key).WaitForCompletion();
			}
			else
			{
				obj.transform.SetParent(null);
				obj.SetActive(true);
			}

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		public GameObject Spawn(Transform parent)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = Addressables.InstantiateAsync(_key, parent).WaitForCompletion();
			}
			else
			{
				obj.transform.SetParent(parent);
				obj.SetActive(true);
			}

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = Addressables.InstantiateAsync(_key, position, rotation).WaitForCompletion();
			}
			else
			{
				obj.transform.SetParent(null);
				obj.transform.SetPositionAndRotation(position, rotation);
				obj.SetActive(true);
			}

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = Addressables.InstantiateAsync(_key, position, rotation, parent).WaitForCompletion();
			}
			else
			{
				obj.transform.SetParent(parent);
				obj.transform.SetPositionAndRotation(position, rotation);
				obj.SetActive(true);
			}

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		public void Despawn(GameObject instance)
		{
			ThrowIfDisposed();

			_stack.Push(instance);
			instance.SetActive(false);
			if (Container != null)
			{
				instance.transform.SetParent(Container.transform);
			}

			PoolCallbackHelper.InvokeOnDespawn(instance);
		}

		public void Prewarm(int count)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var obj = Addressables.InstantiateAsync(_key).WaitForCompletion();
				_stack.Push(obj);
				obj.SetActive(false);
				if (Container != null)
				{
					obj.transform.SetParent(Container.transform);
				}

				PoolCallbackHelper.InvokeOnDespawn(obj);
			}
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				Addressables.ReleaseInstance(obj);
			}
		}

		public void Dispose()
		{
			ThrowIfDisposed();
			Clear();
			_isDisposed = true;
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
