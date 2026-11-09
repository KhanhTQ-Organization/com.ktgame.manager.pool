using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.ktgame.manager.pool
{
    public static class PoolManagerExtensions
    {
        public static void SpawnAndDespawn(this IPoolManager manager, GameObject prefab, float lifeTimeInSeconds, CancellationToken cancellationToken = default)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var instance = manager.Spawn(prefab);
            DespawnAfterDelay(manager, instance, lifeTimeInSeconds, cancellationToken).Forget();
        }
        
        public static void SpawnAndDespawn(this IPoolManager manager, GameObject prefab, Vector3 position, Quaternion rotation, float lifeTimeInSeconds, CancellationToken cancellationToken = default)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            var instance = manager.Spawn(prefab, position, rotation);
            DespawnAfterDelay(manager, instance, lifeTimeInSeconds, cancellationToken).Forget();
        }

        private static async UniTaskVoid DespawnAfterDelay(IPoolManager manager, GameObject instance, float lifeTimeInSeconds, CancellationToken cancellationToken)
        {
            if (instance == null)
            {
                return;
            }
            
            var spawnVersion = PoolInstanceInfo.GetSpawnVersion(instance);
            
            var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, instance.GetCancellationTokenOnDestroy());

            try
            {
                await UniTask.Delay(TimeSpan.FromSeconds(Mathf.Max(0f, lifeTimeInSeconds)), cancellationToken: linkedTokenSource.Token);

                if (instance == null || PoolInstanceInfo.GetSpawnVersion(instance) != spawnVersion)
                {
                    return;
                }

                if (instance.TryGetComponent(out PoolInstanceInfo info) && info.IsInPool)
                {
                    return;
                }

                manager.Despawn(instance);
            }
            catch (OperationCanceledException)
            {
                
            }
            finally
            {
                linkedTokenSource.Dispose();
            }
        }
    }
}
