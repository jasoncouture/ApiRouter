using System;

namespace ApiRouter.Core
{
    public interface IWeakCache
    {
        bool TryGet<T>(string key, out T item) where T : class;
        T GetOrAdd<T>(string key, Func<string, T> createItem) where T : class;
        void Set<T>(string key, T value) where T : class;
    }
}