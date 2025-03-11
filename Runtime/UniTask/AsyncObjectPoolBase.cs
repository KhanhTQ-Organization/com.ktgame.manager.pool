using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

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
		private bool _isDisposed;

		public UniTask<T> SpawnAsync(CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();
			if (_stack.TryPop(out var obj))
			{
				OnSpawn(obj);

				if (obj is ISpawnable spawnable)
				{
					spawnable.OnSpawn();
				}

				if (obj is IPoolable poolable)
				{
					poolable.OnSpawn();
				}

				return new UniTask<T>(obj);
			}

			return CreateInstanceAsync(cancellationToken);
		}

		public void Despawn(T instance)
		{
			ThrowIfDisposed();

			if (instance == null)
			{
				throw new ArgumentNullException(nameof(instance));
			}

			OnDespawn(instance);

			if (instance is IDespawnable despawnable)
			{
				despawnable.OnDespawn();
			}

			if (instance is IPoolable poolable)
			{
				poolable.OnDespawn();
			}

			_stack.Push(instance);
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				OnDestroy(obj);
			}
		}

		public async UniTask PrewarmAsync(int count, CancellationToken cancellationToken = default)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var instance = await CreateInstanceAsync(cancellationToken);
				OnDespawn(instance);
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
