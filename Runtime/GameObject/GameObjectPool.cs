using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	public class GameObjectPool : IPool<GameObject>
	{
		public int Count => _stack.Count;

		public bool IsDisposed => _isDisposed;

		public PoolContainer Container { set; private get; }

		private readonly GameObject _prefab;
		private readonly Stack<GameObject> _stack;
		private bool _isDisposed;

		public GameObjectPool(GameObject prefab, int capacity = 32)
		{
			if (prefab == null)
			{
				throw new ArgumentNullException(nameof(prefab));
			}

			_prefab = prefab;
			_stack = new Stack<GameObject>(capacity);
		}

		public GameObject Spawn()
		{
			ThrowIfDisposed();

			GameObject obj = null;
			while (_stack.Count > 0 && obj == null)
			{
				obj = _stack.Pop();
			}

			if (obj == null)
			{
				obj = UnityEngine.Object.Instantiate(_prefab);
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

			GameObject obj = null;
			while (_stack.Count > 0 && obj == null)
			{
				obj = _stack.Pop();
			}

			if (obj == null)
			{
				obj = UnityEngine.Object.Instantiate(_prefab, parent);
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

			GameObject obj = null;
			while (_stack.Count > 0 && obj == null)
			{
				obj = _stack.Pop();
			}

			if (obj == null)
			{
				obj = UnityEngine.Object.Instantiate(_prefab, position, rotation);
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

			GameObject obj = null;
			while (_stack.Count > 0 && obj == null)
			{
				obj = _stack.Pop();
			}

			if (obj == null)
			{
				obj = UnityEngine.Object.Instantiate(_prefab, position, rotation, parent);
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

		public void Despawn(GameObject obj)
		{
			ThrowIfDisposed();

			if (obj == null) return;

			_stack.Push(obj);
			if (Container != null)
			{
				obj.transform.SetParent(Container.transform);
			}
			obj.SetActive(false);

			PoolCallbackHelper.InvokeOnDespawn(obj);
		}

		public void Prewarm(int count)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var obj = UnityEngine.Object.Instantiate(_prefab);
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
				if (obj != null)
				{
					UnityEngine.Object.Destroy(obj);
				}
			}
		}

		public void Cull(int maxCapacity)
		{
			ThrowIfDisposed();

			while (_stack.Count > maxCapacity)
			{
				if (_stack.TryPop(out var obj) && obj != null)
				{
					UnityEngine.Object.Destroy(obj);
				}
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
