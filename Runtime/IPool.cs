using System;

namespace com.ktgame.manager.pool
{
	public interface IPool : IDisposable { }

	public interface IPool<T> : IPool
	{
		T Spawn();

		void Despawn(T instance);

		void Prewarm(int count);

		void Clear();
	}
}
