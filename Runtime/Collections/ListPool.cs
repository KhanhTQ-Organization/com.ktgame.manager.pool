using System.Collections.Generic;

namespace com.ktgame.manager.pool.Collections
{
    public static class ListPool<T>
    {
        private static readonly Stack<List<T>> _stack = new Stack<List<T>>();

        public static List<T> Get()
        {
            if (_stack.Count > 0)
            {
                return _stack.Pop();
            }
            return new List<T>();
        }

        public static void Release(List<T> list)
        {
            if (list == null) return;
            
            list.Clear();
            _stack.Push(list);
        }
    }
}
