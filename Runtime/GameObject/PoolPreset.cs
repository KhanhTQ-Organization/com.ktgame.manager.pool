using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace com.ktgame.manager.pool
{
	[Serializable]
	public sealed class PoolPreset
	{
		[SerializeField, FoldoutGroup("$_prefab")] private bool _enabled = true;
		[SerializeField, FoldoutGroup("$_prefab")] private GameObject _prefab;
		[SerializeField, FoldoutGroup("$_prefab"), Min(0)] private int _capacity = 32;
		[SerializeField, FoldoutGroup("$_prefab"), Min(0)] private int _preloadSize = 0;

		public bool Enabled => _enabled;

		public GameObject Prefab => _prefab;

		public int Capacity => _capacity;

		public int PreloadSize => _preloadSize;
	}
}
