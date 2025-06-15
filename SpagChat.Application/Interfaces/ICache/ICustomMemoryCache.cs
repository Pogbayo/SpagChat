using Microsoft.Extensions.Caching.Memory;


namespace SpagChat.Application.Interfaces.ICache
{
    public interface ICustomMemoryCache
    {
        void Set(string key, object value, MemoryCacheEntryOptions options);
        bool TryGetValue<T>(string key, out T value);
        void Remove(string key);
        void RemoveByPrefix(string prefix);
    }
}
