﻿namespace CacheManager.Config;

/// <summary>
/// Config of Redis cache
/// </summary>
public abstract class RedisConfig
{
    /// <summary>
    /// Connection String for db connect
    /// </summary>
    public required string ConnectionString { get; init; }
    
    /// <summary>
    /// Cache Time
    /// </summary>
    public required TimeSpan CacheTime { get; init; } = TimeSpan.FromSeconds(5);
}