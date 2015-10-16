using System;
using System.Runtime.Caching;

namespace WebDAVSharp.Data.HelperClasses
{
    /// <summary>
    /// </summary>
    public class CacheBase
    {
        private readonly MemoryCache _cache;
        private readonly object _padlock = new object();
        private readonly CacheItemPolicy _policy;
        private readonly string name;

        /// <summary>
        /// </summary>
        public CacheBase()
        {
            _policy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = DateTimeOffset.Now.AddHours(1.00)
            };
            name = GetType().Name + "_Cache";
            _cache = new MemoryCache(name);
        }

        /// <summary>
        /// </summary>
        /// <param name="policy"></param>
        private CacheBase(CacheItemPolicy policy)
        {
            _policy = policy;
            name = GetType().Name + "_Cache";
            _cache = new MemoryCache(name);
        }

        /// <summary>
        /// </summary>
        public MemoryCache Cache => _cache;

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="remove"></param>
        /// <returns></returns>
        public virtual object GetCachedObject(string key, bool remove = false)
        {
            lock (_padlock)
            {
                object res = _cache[key];
                if (res != null && remove)
                    _cache.Remove(key);
                return res;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public virtual void AddCacheObject(string key, object value)
        {
            lock (_padlock)
            {
                _cache.Set(key, value, _policy);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeToExpire"></param>
        public virtual void AddCacheObject(string key, object value, TimeSpan timeToExpire)
        {
            lock (_padlock)
            {
                CacheItemPolicy itempolicy = new CacheItemPolicy
                {
                    Priority = CacheItemPriority.Default,
                    AbsoluteExpiration = DateTimeOffset.Now.Add(timeToExpire)
                };

                _cache.Set(key, value, _policy);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        public virtual void RemoveCacheObject(string key)
        {
            lock (_padlock)
            {
                _cache.Remove(key);
            }
        }
    }
}