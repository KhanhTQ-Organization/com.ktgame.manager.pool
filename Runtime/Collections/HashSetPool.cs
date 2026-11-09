using System.Collections.Generic;

namespace com.ktgame.manager.pool.Collections
{
    public static class HashSetPool<T>
    {
        private const int MaxPoolSize = 128;

        private static readonly object _lock = new object();
        private static readonly Stack<HashSet<T>> _stack = new Stack<HashSet<T>>();
        private static readonly HashSet<HashSet<T>> _inPool = new HashSet<HashSet<T>>(ReferenceComparer<HashSet<T>>.Instance);

        public static HashSet<T> Get()
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

            return new HashSet<T>();
        }

        public static void Release(HashSet<T> set)
        {
            if (set == null) return;

            set.Clear();

            lock (_lock)
            {
                if (_stack.Count >= MaxPoolSize || !_inPool.Add(set))
                {
                    return;
                }

                _stack.Push(set);
            }
        }
    }
}
