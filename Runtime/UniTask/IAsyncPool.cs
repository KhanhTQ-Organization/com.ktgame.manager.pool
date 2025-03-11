using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.ktgame.manager.pool
{
	public interface IAsyncPool<T> : IDisposable
	{
		UniTask<T> SpawnAsync(CancellationToken cancellationToken);

		void Despawn(T instance);

		UniTask PrewarmAsync(int count, CancellationToken cancellationToken);

		void Clear();
	}
}
