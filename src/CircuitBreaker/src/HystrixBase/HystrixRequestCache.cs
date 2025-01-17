// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Steeltoe.CircuitBreaker.Hystrix.Strategy.Concurrency;
using Steeltoe.Common;
using System;
using System.Collections.Concurrent;

namespace Steeltoe.CircuitBreaker.Hystrix;

public class HystrixRequestCache
{
    // the String key must be: HystrixRequestCache.prefix + cacheKey
    private static readonly ConcurrentDictionary<RequestCacheKey, HystrixRequestCache> Caches = new ();

    private sealed class HystrixRequestCacheVariable : HystrixRequestVariableDefault<ConcurrentDictionary<ValueCacheKey, object>>
    {
        public HystrixRequestCacheVariable()
            : base(() => new ConcurrentDictionary<ValueCacheKey, object>())
        {
        }
    }

    private static readonly HystrixRequestCacheVariable RequestVariableForCache = new ();

    public static HystrixRequestCache GetInstance(IHystrixCommandKey key)
    {
        return GetInstance(new RequestCacheKey(key));
    }

    public static HystrixRequestCache GetInstance(IHystrixCollapserKey key)
    {
        return GetInstance(new RequestCacheKey(key));
    }

    private static HystrixRequestCache GetInstance(RequestCacheKey rcKey)
    {
        return Caches.GetOrAddEx(rcKey, _ => new HystrixRequestCache(rcKey));
    }

    private readonly RequestCacheKey _rcKey;

    private HystrixRequestCache(RequestCacheKey rcKey)
    {
        _rcKey = rcKey;
    }

    public T Get<T>(string cacheKey)
    {
        var key = GetRequestCacheKey(cacheKey);
        if (key != null)
        {
            var cacheInstance = RequestVariableForCache.Value;
            /* look for the stored value */
            if (cacheInstance.TryGetValue(key, out var result))
            {
                return (T)result;
            }
        }

        return default;
    }

    public void Clear(string cacheKey)
    {
        var key = GetRequestCacheKey(cacheKey);
        if (key != null)
        {
            /* remove this cache key */
            var cacheInstance = RequestVariableForCache.Value;
            cacheInstance.TryRemove(key, out _);
        }
    }

    internal T PutIfAbsent<T>(string cacheKey, T f)
    {
        var key = GetRequestCacheKey(cacheKey);
        if (key != null)
        {
            var cacheInstance = RequestVariableForCache.Value;
            var result = cacheInstance.GetOrAdd(key, f);
            if (f.Equals(result))
            {
                return default;
            }
            else
            {
                return (T)result;
            }
        }

        return default;
    }

    private ValueCacheKey GetRequestCacheKey(string cacheKey)
    {
        if (cacheKey != null)
        {
            /* create the cache key we will use to retrieve/store that include the type key prefix */
            return new ValueCacheKey(_rcKey, cacheKey);
        }

        return null;
    }

    private sealed class ValueCacheKey
    {
        private readonly RequestCacheKey _rvKey;
        private readonly string _valueCacheKey;

        public ValueCacheKey(RequestCacheKey rvKey, string valueCacheKey)
        {
            _rvKey = rvKey;
            _valueCacheKey = valueCacheKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_rvKey, _valueCacheKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not ValueCacheKey other)
            {
                return false;
            }

            if (_rvKey == null)
            {
                if (other._rvKey != null)
                {
                    return false;
                }
            }
            else if (!_rvKey.Equals(other._rvKey))
            {
                return false;
            }

            if (_valueCacheKey == null)
            {
                if (other._valueCacheKey != null)
                {
                    return false;
                }
            }
            else if (!_valueCacheKey.Equals(other._valueCacheKey))
            {
                return false;
            }

            return true;
        }
    }

    private sealed class RequestCacheKey
    {
        private readonly short _type; // used to differentiate between Collapser/Command if key is same between them
        private readonly string _key;

        public RequestCacheKey(IHystrixCommandKey commandKey)
        {
            _type = 1;
            _key = commandKey?.Name;
        }

        public RequestCacheKey(IHystrixCollapserKey collapserKey)
        {
            _type = 2;
            _key = collapserKey?.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_key, _type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not RequestCacheKey other)
            {
                return false;
            }

            if (_type != other._type)
            {
                return false;
            }

            if (_key == null)
            {
                if (other._key != null)
                {
                    return false;
                }
            }
            else if (!_key.Equals(other._key))
            {
                return false;
            }

            return true;
        }
    }
}
