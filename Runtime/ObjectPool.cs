using System;

namespace com.ktgame.manager.pool
{
	public sealed class ObjectPool<T> : PoolBase<T> where T : class
	{
		private readonly Func<T> _createFunc;
		private readonly Action<T> _onSpawn;
		private readonly Action<T> _onDespawn;
		private readonly Action<T> _onDestroy;

		public ObjectPool(Func<T> createFunc, Action<T> onSpawn = null, Action<T> onDespawn = null, Action<T> onDestroy = null)
		{
			_createFunc = createFunc ?? throw new ArgumentException(nameof(createFunc));
			_onSpawn = onSpawn;
			_onDespawn = onDespawn;
			_onDestroy = onDestroy;
		}

		protected override T CreateInstance()
		{
			return _createFunc();
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
