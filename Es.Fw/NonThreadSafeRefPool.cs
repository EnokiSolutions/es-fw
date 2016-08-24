using System;
using Es.FwI;

namespace Es.Fw
{
    public sealed class NonThreadSafeRefPool<T> : IRefPool<T> where T : class
    {
        private T[] _items;
        private int _n;
        private readonly Func<T> _ctor;

        public NonThreadSafeRefPool(Func<T> ctor, int initialCap = 16)
        {
            _ctor = ctor;
            _items = new T[initialCap];
            _n = 0;
        }

        public T Acquire()
        {
            try
            {
                var r = _items[_n] ?? _ctor();
                _items[_n] = null;
                ++_n;
                return r;
            }
            catch
            {
                _items = new T[_items.Length + _items.Length/2]; // 1.5x
                var r = _items[_n] ?? _ctor();
                _items[_n] = null;
                _n++;
                return r;
            }
        }

        public void Release(T t)
        {
            _items[--_n] = t;
        }
    }
}