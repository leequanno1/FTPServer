using System;

namespace lib
{
    public sealed class ItemSingleton<T>
    {
        private static ItemSingleton<T> _instance;
        private static readonly object _lock = new object();
        private T value;

        private ItemSingleton()
        {
            value = default;
        }

        public static ItemSingleton<T> GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null) // Double-checked locking
                    {
                        _instance = new ItemSingleton<T>();
                    }
                }
            }
            return _instance;
        }

        public void Update(T newValue)
        {
            value = newValue;
        }

        public T GetValue()
        {
            return value;
        }
    }
}
