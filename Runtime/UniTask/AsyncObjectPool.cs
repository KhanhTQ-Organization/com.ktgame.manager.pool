using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.ktgame.manager.pool
{
	public sealed class AsyncObjectPool<T> : AsyncObjectPoolBase<T> where T : class
	{
		private readonly Func<CancellationToken, UniTask<T>> _createFunc;
		private readonly Action<T> _onSpawn;
		private readonly Action<T> _onDespawn;
		private readonly Action<T> _onDestroy;

		public AsyncObjectPool(Func<UniTask<T>> createFunc, Action<T> onSpawn = null, Action<T> onDespawn = null, Action<T> onDestroy = null)
		{
			if (createFunc == null)
			{
				throw new ArgumentException(nameof(createFunc));
			}

			_createFunc = _ => createFunc();
			_onSpawn = onSpawn;
			_onDespawn = onDespawn;
			_onDestroy = onDestroy;
		}

		public AsyncObjectPool(Func<CancellationToken, UniTask<T>> createFunc, Action<T> onSpawn = null, Action<T> onDespawn = null, Action<T> onDestroy = null)
		{
			_createFunc = createFunc ?? throw new ArgumentException(nameof(createFunc));
			_onSpawn = onSpawn;
			_onDespawn = onDespawn;
			_onDestroy = onDestroy;
		}

		protected override UniTask<T> CreateInstanceAsync(CancellationToken cancellationToken)
		{
			return _createFunc(cancellationToken);
		}

		protected override void OnDestroy(T instance)
		{
			_onDestroy?.Invoke(instance);
		}

		protected override void OnSpawn(T instance)
		{
			_onSpawn?.Invoke(instance);
		}

		protected override void OnDespawn(T instance)
		{
			_onDespawn?.Invoke(instance);
		}
	}
}
