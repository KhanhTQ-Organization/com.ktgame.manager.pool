using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	public abstract class PoolBase<T> : IPool<T> where T : class
	{
		public Stack<T> Stack => _stack;

		public int Count => _stack.Count;

		public bool IsDisposed => _isDisposed;

		protected abstract T CreateInstance();
		protected virtual void OnDestroy(T instance) { }
		protected virtual void OnSpawn(T instance) { }
		protected virtual void OnDespawn(T instance) { }

		private readonly Stack<T> _stack = new(32);
		private bool _isDisposed;

		public T Spawn()
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

				return obj;
			}

			return CreateInstance();
		}

		public void Despawn(T obj)
		{
			ThrowIfDisposed();

			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			OnDespawn(obj);

			if (obj is IDespawnable despawnable)
			{
				despawnable.OnDespawn();
			}

			if (obj is IPoolable poolable)
			{
				poolable.OnDespawn();
			}

			_stack.Push(obj);
		}

		public void Prewarm(int count)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var instance = CreateInstance();
				Despawn(instance);
			}
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				OnDestroy(obj);
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
