using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.ktgame.manager.pool
{
    public static class PoolManagerExtensions
    {
        /// <summary>
        /// Spawns a prefab and automatically despawns it after a specified lifetime.
        /// </summary>
        public static void SpawnAndDespawn(this IPoolManager manager, GameObject prefab, float lifeTimeInSeconds, CancellationToken cancellationToken = default)
        {
            var instance = manager.Spawn(prefab);
            DespawnAfterDelay(manager, instance, lifeTimeInSeconds, cancellationToken).Forget();
        }

        /// <summary>
        /// Spawns a prefab at a specific position/rotation and automatically despawns it after a specified lifetime.
        /// </summary>
        public static void SpawnAndDespawn(this IPoolManager manager, GameObject prefab, Vector3 position, Quaternion rotation, float lifeTimeInSeconds, CancellationToken cancellationToken = default)
        {
            var instance = manager.Spawn(prefab, position, rotation);
            DespawnAfterDelay(manager, instance, lifeTimeInSeconds, cancellationToken).Forget();
        }

        private static async UniTaskVoid DespawnAfterDelay(IPoolManager manager, GameObject instance, float lifeTimeInSeconds, CancellationToken cancellationToken)
        {
            // Link the cancellation token to the instance's destruction to prevent memory leaks or errors
            // if the object is manually destroyed before the delay completes.
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, instance.GetCancellationTokenOnDestroy());

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(lifeTimeInSeconds), cancellationToken: linkedTokenSource.Token);
                manager.Despawn(instance);
            }
            catch (OperationCanceledException)
            {
                // Ignored. Object was destroyed manually or task was cancelled.
            }
            finally
            {
                linkedTokenSource.Dispose();
            }
        }
    }
}
