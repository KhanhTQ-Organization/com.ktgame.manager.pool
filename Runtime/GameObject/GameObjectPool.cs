using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
				obj = UnityEngine.Object.Instantiate(_prefab);
			}
			else
			{
				obj.transform.SetParent(null, false);
				ResetLocalTransform(obj.transform);
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
				obj = UnityEngine.Object.Instantiate(_prefab, parent);
			}
			else
			{
				obj.transform.SetParent(parent, false);
				ResetLocalTransform(obj.transform);
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
				obj = UnityEngine.Object.Instantiate(_prefab, position, rotation);
			}
			else
			{
				obj.transform.SetParent(null, false);
				ResetLocalTransform(obj.transform);
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
				obj = UnityEngine.Object.Instantiate(_prefab, position, rotation, parent);
			}
			else
			{
				obj.transform.SetParent(parent, false);
				ResetLocalTransform(obj.transform);
				obj.transform.SetPositionAndRotation(position, rotation);
				obj.SetActive(true);
			}

			return OnSpawned(obj);
		}

		public void Despawn(GameObject obj)
		{
			ThrowIfDisposed();

			if (obj == null) return;

			var info = PoolInstanceInfo.GetOrAdd(obj, this);
			if (info.IsInPool)
			{
				LogWarning($"[Pool] '{obj.name}' is already in the pool - duplicate Despawn ignored.", obj);
				return;
			}
			
			PoolCallbackHelper.InvokeOnDespawn(obj);

			obj.SetActive(false);
			if (Container != null)
			{
				obj.transform.SetParent(Container.transform, false);
			}

			info.IsInPool = true;
			_stack.Push(obj);
		}

		public void Prewarm(int count)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var obj = Container != null
					? UnityEngine.Object.Instantiate(_prefab, Container.transform, false)
					: UnityEngine.Object.Instantiate(_prefab);

				var info = PoolInstanceInfo.GetOrAdd(obj, this);
				PoolCallbackHelper.InvokeOnDespawn(obj);
				obj.SetActive(false);
				info.IsInPool = true;
				_stack.Push(obj);
			}
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				DestroyObject(obj);
			}
		}

		public void Cull(int maxCapacity)
		{
			ThrowIfDisposed();

			maxCapacity = Mathf.Max(0, maxCapacity);
			while (_stack.Count > maxCapacity && _stack.TryPop(out var obj))
			{
				DestroyObject(obj);
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
		
		private void ResetLocalTransform(Transform target)
		{
			var source = _prefab.transform;
			if (source is RectTransform sourceRect && target is RectTransform targetRect)
			{
				targetRect.anchorMin = sourceRect.anchorMin;
				targetRect.anchorMax = sourceRect.anchorMax;
				targetRect.pivot = sourceRect.pivot;
				targetRect.sizeDelta = sourceRect.sizeDelta;
				targetRect.anchoredPosition3D = sourceRect.anchoredPosition3D;
			}
			else
			{
				target.localPosition = source.localPosition;
			}

			target.localRotation = source.localRotation;
			target.localScale = source.localScale;
		}

		private static void DestroyObject(GameObject obj)
		{
			if (obj == null)
			{
				return;
			}

			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(obj);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(obj);
			}
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
