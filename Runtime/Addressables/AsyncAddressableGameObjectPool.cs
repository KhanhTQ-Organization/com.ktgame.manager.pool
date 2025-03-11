using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace com.ktgame.manager.pool
{
	public sealed class AsyncAddressableGameObjectPool : IAsyncPool<GameObject>
	{
		public int Count => _stack.Count;

		public bool IsDisposed => _isDisposed;

		public PoolContainer Container { set; private get; }

		private readonly object _key;
		private readonly Stack<GameObject> _stack;
		private bool _isDisposed;

		public AsyncAddressableGameObjectPool(object key, int capacity = 32)
		{
			_key = key;
			_stack = new Stack<GameObject>(capacity);
		}

		public AsyncAddressableGameObjectPool(AssetReferenceGameObject reference, int capacity = 32)
		{
			_key = reference.RuntimeKey;
			_stack = new Stack<GameObject>(capacity);
		}

		public async UniTask<GameObject> SpawnAsync(CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = await Addressables.InstantiateAsync(_key).ToUniTask(cancellationToken: cancellationToken);
			}
			else
			{
				obj.transform.SetParent(null);
				obj.SetActive(true);
			}

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		public async UniTask<GameObject> SpawnAsync(Transform parent, CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = await Addressables.InstantiateAsync(_key, parent).ToUniTask(cancellationToken: cancellationToken);
			}
			else
			{
				obj.transform.SetParent(parent);
				obj.SetActive(true);
			}

			PoolCallbackHelper.InvokeOnSpawn(obj);
			return obj;
		}

		public async UniTask<GameObject> SpawnAsync(Vector3 position, Quaternion rotation, CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = await Addressables.InstantiateAsync(_key, position, rotation).ToUniTask(cancellationToken: cancellationToken);
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

		public async UniTask<GameObject> SpawnAsync(Vector3 position, Quaternion rotation, Transform parent)
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = await Addressables.InstantiateAsync(_key, position, rotation, parent);
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

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				Addressables.ReleaseInstance(obj);
			}
		}

		public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var obj = await Addressables.InstantiateAsync(_key).ToUniTask(cancellationToken: cancellationToken);
				_stack.Push(obj);
				obj.SetActive(false);
				if (Container != null)
				{
					obj.transform.SetParent(Container.transform);
				}

				PoolCallbackHelper.InvokeOnDespawn(obj);
			}
		}

		public void Dispose()
		{
			throw new NotImplementedException();
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
