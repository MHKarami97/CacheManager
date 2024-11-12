﻿using CacheManager.CacheSource;
using CacheManager.Config;

namespace CacheManager;

/// <summary>
/// Simple cache builder
/// </summary>
/// <typeparam name="T">EasyCacheManager</typeparam>
public class CacheBuilder<T>
{
    private readonly List<IBaseCacheSource<T>> _cacheSources = [];

    /// <summary>
    /// Add Memory Cache
    /// </summary>
    /// <param name="memoryConfig">Config</param>
    /// <returns>CacheBuilder</returns>
    public CacheBuilder<T> AddMemory(MemoryConfig memoryConfig)
    {
        _cacheSources.Add(new MemoryCacheSource<T>(memoryConfig));
        return this;
    }

    /// <summary>
    /// Add Redis Cache
    /// </summary>
    /// <param name="redisConfig">Config</param>
    /// <returns>CacheBuilder</returns>
    public CacheBuilder<T> AddRedis(RedisConfig redisConfig)
    {
        _cacheSources.Add(new RedisCacheSource<T>(redisConfig));
        return this;
    }

    /// <summary>
    /// Add Sql Server Cache
    /// </summary>
    /// <param name="dbConfig">Config</param>
    /// <returns>CacheBuilder</returns>
    public CacheBuilder<T> AddDb(DbConfig dbConfig)
    {
        _cacheSources.Add(new DbCacheSource<T>(dbConfig));
        return this;
    }
    
    /// <summary>
    /// Add Api Cache
    /// </summary>
    /// <param name="apiConfig">Config</param>
    /// <returns>CacheBuilder</returns>
    public CacheBuilder<T> AddApi(ApiConfig apiConfig)
    {
        _cacheSources.Add(new ApiCacheSource<T>(apiConfig));
        return this;
    }

    /// <summary>
    /// Build EasyCacheManager
    /// </summary>
    /// <param name="lockConfig">Config</param>
    /// <returns>EasyCacheManager</returns>
    public EasyCacheManager<T> Build(LockConfig lockConfig)
    {
        return new EasyCacheManager<T>(_cacheSources, lockConfig);
    }
}