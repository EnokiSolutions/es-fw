using System.Threading;

namespace Es.Fw
{
    // No interface here, this is performance code.
    public sealed class AtomicAppendPeelQueue<T> where T: AtomicAppendPeelQueueNode
    {
        // Using base class inheritence to avoid data pointer and 
        // inline _next with the data as a performance optimization

        private T _head;

        public void AppendList(T head, T tail)
        {
            tail.Next = _head;
            for (;;)
            {
                var oldValue = Interlocked.CompareExchange(ref _head, head, (T)tail.Next);
                if (oldValue == tail.Next)
                    break;

                // _head was changed, update and retry
                tail.Next = oldValue;
            }
        }

        public T Peel()
        {
            if (_head == null)
                return null;

            var temp = _head;
            for (;;)
            {
                var oldValue = Interlocked.CompareExchange(ref _head, null, temp);
                if (oldValue == temp)
                    return temp;
                if (oldValue == null)
                    return null;
                temp = oldValue;
            }
        }
    }
}