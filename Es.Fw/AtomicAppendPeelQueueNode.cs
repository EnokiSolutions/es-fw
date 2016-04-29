namespace Es.Fw
{
    // One of the few valid uses of inheritence.
    // Also strictly for a performance case.
    public abstract class AtomicAppendPeelQueueNode
    {
        public AtomicAppendPeelQueueNode Next;

        public static void Push<T>(ref T head, ref T tail, T data) where T: AtomicAppendPeelQueueNode
        {
            if (head == null)
            {
                head = data;
                tail = data;
                return;
            }
            data.Next = head;
            head = data;
        }
    }
}