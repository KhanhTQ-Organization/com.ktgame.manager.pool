using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
		private readonly HashSet<T> _inPool = new(ReferenceComparer<T>.Instance);
		private bool _isDisposed;
		
		public bool Contains(T instance)
		{
			return instance != null && _inPool.Contains(instance);
		}

		public T Spawn()
		{
			ThrowIfDisposed();

			if (!_stack.TryPop(out var obj))
			{
				obj = CreateInstance();
			}
			else
			{
				_inPool.Remove(obj);
			}
			
			if (obj != null)
			{
				OnSpawn(obj);
				PoolCallbackHelper.InvokeOnSpawnObject(obj);
			}

			return obj;
		}

		public void Despawn(T obj)
		{
			ThrowIfDisposed();

			if (obj == null)
			{
				throw new ArgumentNullException(nameof(obj));
			}

			if (!_inPool.Add(obj))
			{
				LogWarning($"[Pool] {typeof(T).Name} instance is already in the pool - duplicate Despawn ignored.");
				return;
			}

			OnDespawn(obj);
			PoolCallbackHelper.InvokeOnDespawnObject(obj);

			_stack.Push(obj);
		}

		public void Prewarm(int count)
		{
			ThrowIfDisposed();

			for (var i = 0; i < count; i++)
			{
				var instance = CreateInstance();
				if (instance != null)
				{
					Despawn(instance);
				}
			}
		}

		public void Clear()
		{
			ThrowIfDisposed();

			while (_stack.TryPop(out var obj))
			{
				_inPool.Remove(obj);
				OnDestroy(obj);
			}

			_inPool.Clear();
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
