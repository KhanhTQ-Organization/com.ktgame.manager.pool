using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	internal static class PoolCallbackHelper
	{
		private static readonly Stack<List<ISpawnable>> SpawnBuffers = new Stack<List<ISpawnable>>();
		private static readonly Stack<List<IDespawnable>> DespawnBuffers = new Stack<List<IDespawnable>>();

		public static void InvokeOnSpawn(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return;
			}

			var buffer = SpawnBuffers.Count > 0 ? SpawnBuffers.Pop() : new List<ISpawnable>(8);
			try
			{
				gameObject.GetComponentsInChildren(true, buffer);
				for (var i = 0; i < buffer.Count; i++)
				{
					try
					{
						buffer[i].OnSpawn();
					}
					catch (Exception e)
					{
						Debug.LogException(e, buffer[i] as UnityEngine.Object);
					}
				}
			}
			finally
			{
				buffer.Clear();
				SpawnBuffers.Push(buffer);
			}
		}

		public static void InvokeOnDespawn(GameObject gameObject)
		{
			if (gameObject == null)
			{
				return;
			}

			var buffer = DespawnBuffers.Count > 0 ? DespawnBuffers.Pop() : new List<IDespawnable>(8);
			try
			{
				gameObject.GetComponentsInChildren(true, buffer);
				for (var i = 0; i < buffer.Count; i++)
				{
					try
					{
						buffer[i].OnDespawn();
					}
					catch (Exception e)
					{
						Debug.LogException(e, buffer[i] as UnityEngine.Object);
					}
				}
			}
			finally
			{
				buffer.Clear();
				DespawnBuffers.Push(buffer);
			}
		}
		
		public static void InvokeOnSpawnObject(object obj)
		{
			if (obj is ISpawnable spawnable)
			{
				spawnable.OnSpawn();
			}
		}
		
		public static void InvokeOnDespawnObject(object obj)
		{
			if (obj is IDespawnable despawnable)
			{
				despawnable.OnDespawn();
			}
		}
	}
	
	internal sealed class ReferenceComparer<T> : IEqualityComparer<T> where T : class
	{
		public static readonly ReferenceComparer<T> Instance = new ReferenceComparer<T>();

		public bool Equals(T x, T y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(T obj)
		{
			return System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
		}
	}
}
