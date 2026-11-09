using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace com.ktgame.manager.pool
{
	internal static class AddressablePoolUtility
	{
		public static GameObject Instantiate(AsyncOperationHandle<GameObject> handle, object key)
		{
			var obj = handle.WaitForCompletion();
			if (obj == null)
			{
				Release(handle);
				throw new InvalidOperationException($"[Pool] Cannot instantiate addressable '{key}'.");
			}

			return obj;
		}

		public static async UniTask<GameObject> InstantiateAsync(AsyncOperationHandle<GameObject> handle, object key, CancellationToken cancellationToken)
		{
			GameObject obj;
			try
			{
				obj = await handle.ToUniTask(cancellationToken: cancellationToken);
			}
			catch (OperationCanceledException)
			{
				ReleaseWhenDone(handle);
				throw;
			}

			if (obj == null)
			{
				Release(handle);
				throw new InvalidOperationException($"[Pool] Cannot instantiate addressable '{key}'.");
			}

			return obj;
		}

		public static void ReleaseInstance(GameObject obj)
		{
			if (obj == null)
			{
				return;
			}

			if (!Addressables.ReleaseInstance(obj))
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(obj);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(obj);
				}
			}
		}

		private static void ReleaseWhenDone(AsyncOperationHandle<GameObject> handle)
		{
			if (!handle.IsValid())
			{
				return;
			}

			if (handle.IsDone)
			{
				Release(handle);
			}
			else
			{
				handle.Completed += Release;
			}
		}

		private static void Release(AsyncOperationHandle<GameObject> handle)
		{
			if (!handle.IsValid())
			{
				return;
			}

			if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
			{
				Addressables.ReleaseInstance(handle.Result);
			}
			else
			{
				Addressables.Release(handle);
			}
		}
	}
	
	internal sealed class TransformSnapshot
	{
		private readonly bool _isRect;
		private readonly Vector3 _localPosition;
		private readonly Quaternion _localRotation;
		private readonly Vector3 _localScale;
		private readonly Vector2 _anchorMin;
		private readonly Vector2 _anchorMax;
		private readonly Vector2 _pivot;
		private readonly Vector2 _sizeDelta;
		private readonly Vector3 _anchoredPosition3D;

		public TransformSnapshot(Transform source)
		{
			_localPosition = source.localPosition;
			_localRotation = source.localRotation;
			_localScale = source.localScale;

			if (source is RectTransform rect)
			{
				_isRect = true;
				_anchorMin = rect.anchorMin;
				_anchorMax = rect.anchorMax;
				_pivot = rect.pivot;
				_sizeDelta = rect.sizeDelta;
				_anchoredPosition3D = rect.anchoredPosition3D;
			}
		}

		public void ApplyTo(Transform target)
		{
			if (_isRect && target is RectTransform rect)
			{
				rect.anchorMin = _anchorMin;
				rect.anchorMax = _anchorMax;
				rect.pivot = _pivot;
				rect.sizeDelta = _sizeDelta;
				rect.anchoredPosition3D = _anchoredPosition3D;
			}
			else
			{
				target.localPosition = _localPosition;
			}

			target.localRotation = _localRotation;
			target.localScale = _localScale;
		}
	}
}
