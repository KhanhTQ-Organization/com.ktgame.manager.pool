using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace com.ktgame.manager.pool
{
	public abstract class AsyncObjectPoolBase<T> : IAsyncPool<T> where T : class
	{
		public int Count => _stack.Count;

		public bool IsDisposed => _isDisposed;

		protected abstract UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken);
		protected virtual void OnDestroy(T instance) { }
		protected virtual void OnSpawn(T instance) { }
		protected virtual void OnDespawn(T instance) { }

		private readonly Stack<T> _stack = new(32);
		private readonly HashSet<T> _inPool = new(ReferenceComparer<T>.Instance);
		private bool _isDisposed;
		
		public bool Contains(T instance)
		{
			return instance != null && _inPool.Contains(instance);
		}

		public UniTask<T> SpawnAsync(CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			if (_stack.TryPop(out var obj))
			{
				_inPool.Remove(obj);
				InvokeSpawnCallbacks(obj);
				return new UniTask<T>(obj);
			}

			return CreateAndSpawnAsync(cancellationToken);
		}

		private async UniTask<T> CreateAndSpawnAsync(CancellationToken cancellationToken)
		{
			var obj = await CreateInstanceAsync(cancellationToken);
			InvokeSpawnCallbacks(obj);
			return obj;
		}

		private void InvokeSpawnCallbacks(T obj)
		{
			if (obj == null)
			{
				return;
			}

			OnSpawn(obj);
			PoolCallbackHelper.InvokeOnSpawnObject(obj);
		}

		public void Despawn(T instance)
		{
			ThrowIfDisposed();

			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			if (!_inPool.Add(instance))
			{
				LogWarning($"[Pool] {typeof(T).Name} instance is already in the pool - duplicate Despawn ignored.");
				return;
			}

			OnDespawn(instance);
			PoolCallbackHelper.InvokeOnDespawnObject(instance);

			_stack.Push(instance);
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				OnDestroy(obj);
			}

			_inPool.Clear();
		}

		public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var instance = await CreateInstanceAsync(cancellationToken);
				if (instance == null)
				{
					continue;
				}

				if (_isDisposed)
				{
					OnDestroy(instance);
					return;
				}
				
				Despawn(instance);
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

		[Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT")]
		private static void LogWarning(string message)
		{
			Debug.LogWarning(message);
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
