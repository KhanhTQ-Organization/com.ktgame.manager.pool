using System.Collections.Generic;

namespace com.ktgame.manager.pool.Collections
{
    public static class DictionaryPool<TKey, TValue>
    {
        private static readonly Stack<Dictionary<TKey, TValue>> _stack = new Stack<Dictionary<TKey, TValue>>();

        public static Dictionary<TKey, TValue> Get()
        {
            if (_stack.Count > 0)
            {
                return _stack.Pop();
            }
            return new Dictionary<TKey, TValue>();
        }

        public static void Release(Dictionary<TKey, TValue> dict)
        {
            if (dict == null) return;

            dict.Clear();
            _stack.Push(dict);
        }
    }
}
