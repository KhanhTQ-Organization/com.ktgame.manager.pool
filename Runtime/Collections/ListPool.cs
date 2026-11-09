using System.Collections.Generic;

namespace com.ktgame.manager.pool.Collections
{
    public static class ListPool<T>
    {
        private const int MaxPoolSize = 128;

        private static readonly object _lock = new object();
        private static readonly Stack<List<T>> _stack = new Stack<List<T>>();
        private static readonly HashSet<List<T>> _inPool = new HashSet<List<T>>(ReferenceComparer<List<T>>.Instance);

        public static List<T> Get()
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

            return new List<T>();
        }

        public static void Release(List<T> list)
        {
            if (list == null) return;

            list.Clear();

            lock (_lock)
            {
                if (_stack.Count >= MaxPoolSize || !_inPool.Add(list))
                {
                    return;
                }

                _stack.Push(list);
            }
        }
    }
}
