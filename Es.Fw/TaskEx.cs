using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Es.Fw
{
    [ExcludeFromCodeCoverage]
    public static class TaskEx
    {
        public static readonly Task Done = Task.Run(() => { });
        public static readonly Task<bool> False = Task.Run(() => false);
        public static readonly Task<bool> True = Task.Run(() => true);
        public static readonly Task<int> Zero = Task.Run(() => 0);
        public static readonly Task<int> One = Task.Run(() => 1);

        public static Task Background(this Task t)
        {
            t.ConfigureAwait(false);
            return t;
        }

        public static Task<T> Background<T>(this Task<T> t)
        {
            t.ConfigureAwait(false);
            return t;
        }
    }
}