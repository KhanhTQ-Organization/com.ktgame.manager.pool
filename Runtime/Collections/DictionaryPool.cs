using System.Collections.Generic;

namespace com.ktgame.manager.pool.Collections
{
    public static class DictionaryPool<TKey, TValue>
    {
        private const int MaxPoolSize = 128;

        private static readonly object _lock = new object();
        private static readonly Stack<Dictionary<TKey, TValue>> _stack = new Stack<Dictionary<TKey, TValue>>();
        private static readonly HashSet<Dictionary<TKey, TValue>> _inPool = new HashSet<Dictionary<TKey, TValue>>(ReferenceComparer<Dictionary<TKey, TValue>>.Instance);

        public static Dictionary<TKey, TValue> Get()
        {
            lock (_lock)
            {
                if (_stack.Count > 0)
                {
                    var item = _stack.Pop();
                    _inPool.Remove(item);
                    return item;
                }
            }

            return new Dictionary<TKey, TValue>();
        }

        public static void Release(Dictionary<TKey, TValue> dict)
        {
            if (dict == null) return;

            dict.Clear();

            lock (_lock)
            {
                if (_stack.Count >= MaxPoolSize || !_inPool.Add(dict))
                {
                    return;
                }

                _stack.Push(dict);
            }
        }
    }
}
