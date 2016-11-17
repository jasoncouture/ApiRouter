using System;
using System.Collections.Concurrent;
using Castle.Core.Logging;

namespace ApiRouter.Core
{
    public class WeakCache : IWeakCache
    {
        private readonly IConfiguration _configuration;

        public WeakCache(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;
        private readonly ConcurrentDictionary<string, WeakReference> _internalCache = new ConcurrentDictionary<string, WeakReference>(StringComparer.InvariantCultureIgnoreCase);
        public bool TryGet<T>(string key, out T item) where T : class
        {
            if (!_configuration.GetCacheEnabled().GetAwaiter().GetResult())
            {
                item = null;
                return false;
            }
            WeakReference weakReference;
            if (_internalCache.TryGetValue(key, out weakReference) && weakReference.IsAlive)
            {
                Logger.Debug($"Cache hit: {typeof(T).Name}/{key}");
                item = weakReference.Target as T;
                return true;
            }
            item = null;
            Logger.Debug($"Cache miss: {typeof(T).Name}/{key}");
            return false;
        }

        public T GetOrAdd<T>(string key, Func<string, T> createItem) where T : class
        {
            if (!_configuration.GetCacheEnabled().GetAwaiter().GetResult()) return createItem(key);
            while (true)
            {
                var result = _internalCache.GetOrAdd(key, k => new WeakReference(createItem(k)));
                if (result.IsAlive)
                {
                    Logger.Debug($"Cache hit: {typeof(T).Name}/{key}");
                    return result.Target as T;
                }
                Logger.Debug($"Cache miss: {typeof(T).Name}/{key}");
                _internalCache.TryRemove(key, out result);
            }
        }

        public void Set<T>(string key, T value) where T : class
        {
            if (!_configuration.GetCacheEnabled().GetAwaiter().GetResult()) return;
            Logger.Debug($"Cache direct write: {typeof(T).Name}/{key}");
            _internalCache[key] = new WeakReference(value);
        }
    }
}