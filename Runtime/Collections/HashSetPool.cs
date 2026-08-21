using System.Collections.Generic;

namespace com.ktgame.manager.pool.Collections
{
    public static class HashSetPool<T>
    {
        private static readonly Stack<HashSet<T>> _stack = new Stack<HashSet<T>>();

        public static HashSet<T> Get()
        {
            if (_stack.Count > 0)
            {
                return _stack.Pop();
            }
            return new HashSet<T>();
        }

        public static void Release(HashSet<T> set)
        {
            if (set == null) return;
            
            set.Clear();
            _stack.Push(set);
        }
    }
}
