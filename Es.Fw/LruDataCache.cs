using System.Collections.Generic;

namespace Es.Fw
{
    public sealed class LruDataCache
    {
        private sealed class Entry
        {
            public object Value;
            public ulong Weight;
            public ulong Clock;
        }

        private readonly IDictionary<string, Entry> _cache = new Dictionary<string, Entry>();
        private readonly ulong _highWeight;
        private readonly ulong _lowWeight;
        private ulong _totalWeight;
        private ulong _clock;
        private ulong _lastTrimClock;
        private readonly List<string> _removalList = new List<string>();
        private readonly int _index;

        public LruDataCache(ulong lowWeight, ulong highWeight, int index)
        {
            _highWeight = highWeight;
            _index = index;
            _lowWeight = lowWeight;
            _totalWeight = 0;
            _clock = 0;
            _lastTrimClock = 0;
        }

        public void Trim()
        {
            // We only trim items at least one trim generation old.
            //
            // WHY?
            // 1) We do this because we want to ensure relatively new items (in the gap between 
            //    high and low watermark) are kept alive long enough that operations
            //    that rely on the cached items living for a short while to work efficiently
            //    don't accidently degrade in performance.
            // 2) We sometimes use the cache as a short lived locking mechanism (inserts into
            //    a unique index). The cache lifetime of the lock is very small (back to back
            //    RPC operations) and this help to ensure the lock lives long enough.
            //

            if (_totalWeight < _highWeight)
                return;

            var e = _cache.GetEnumerator();
            var iMax = _cache.Count / 3;
            for (var i = 0; i < iMax && _totalWeight > _lowWeight; ++i)
            {
                e.MoveNext();
                var kvp1 = e.Current;
                e.MoveNext();
                var kvp2 = e.Current;
                e.MoveNext();
                var kvp3 = e.Current;

                var k1 = kvp1.Key;
                var k2 = kvp2.Key;
                var k3 = kvp3.Key;

                var e1 = kvp1.Value;
                var e2 = kvp2.Value;
                var e3 = kvp3.Value;

                var c1 = e1.Clock;
                var c2 = e2.Clock;
                var c3 = e3.Clock;

                if (c1 < _lastTrimClock && c1 < c2 && c1 < c3)
                {
                    _totalWeight -= e1.Weight;
                    _removalList.Add(k1);
                }
                else if (c2 < _lastTrimClock && c2 < c3)
                {
                    _totalWeight -= e2.Weight;
                    _removalList.Add(k2);
                }
                else if (c3 < _lastTrimClock)
                {
                    _totalWeight -= e3.Weight;
                    _removalList.Add(k3);
                }
            }

            for (var i = 0; i < _removalList.Count; ++i)
                _cache.Remove(_removalList[i]);

            _removalList.Clear();
            _lastTrimClock = _clock;
        }

        public void Set(string key, object value, uint weight)
        {
            ++_clock;
            _totalWeight += weight;
            var e = _cache.Vivify(key);
            e.Weight = weight;
            e.Value = value;
            e.Clock = _clock;
        }

        public object Get(string key)
        {
            ++_clock;
            Entry v;

            if (!_cache.TryGetValue(key, out v))
            {
                return null;
            }
            v.Clock = _clock;
            return v.Value;
        }

        public void Del(string key)
        {
            Entry v;
            if (!_cache.TryGetValue(key, out v))
                return;

            _totalWeight -= v.Weight;
            _cache.Remove(key);
        }

        public void Flush()
        {
            _clock = 0;
            _lastTrimClock = 0;
            _totalWeight = 0;
            _cache.Clear();
        }
    }
}