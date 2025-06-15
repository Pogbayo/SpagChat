﻿using Microsoft.Extensions.Caching.Memory;
using SpagChat.Application.Interfaces.ICache;

namespace SpagChat.Application.MemoryCache
{
    public class CustomMemoryCache : ICustomMemoryCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HashSet<string> _cacheKeys = new();

        public CustomMemoryCache(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _cacheKeys.Remove(key);
        }

        public void RemoveByPrefix(string prefix)
        {
            var keysToRemove = _cacheKeys.Where(k => k.StartsWith(prefix)).ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
            }
        }

        public void Set(string key, object value, MemoryCacheEntryOptions options)
        {
            _memoryCache.Set(key, value, options);
            _cacheKeys.Add(key);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value!);
        }
    }
}
