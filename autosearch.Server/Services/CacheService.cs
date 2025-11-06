using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using autosearch.Data;
using autosearch.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace autosearch.Services;

/// <summary>
/// centeral cache/memory for the app
/// </summary>
public interface ICacheService
{
    Task SetAsync(string key, JsonArray value, TimeSpan? expiry = null);
    Task<JsonArray?> GetAsync(string key);
    Task DeleteAsync(string key);
}


public class MemoryCacheService : ICacheService
{
    private static MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    public async Task<JsonArray?> GetAsync(string key)
    {
        return await Task.FromResult(_cache.Get<JsonArray>(key));
    }
    public async Task SetAsync(string key, JsonArray value, TimeSpan? expiry = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiry.HasValue)
            options.SetAbsoluteExpiration(expiry.Value);

        _cache.Set(key, value, options);
        await Task.CompletedTask;
    }

    public async Task DeleteAsync(string key)
    {
        _cache.Remove(key);
        await Task.CompletedTask;
    }
}

// public class RedisCacheService : ICacheService
// {
//     private readonly IDatabase _cache;
//     public RedisCacheService(string connectionString = "localhost:6379")
// {
//         //            _cache = ConnectionMultiplexer.Connect(connectionString).GetDatabase();
//         // ensure we don't throw if initial connect fails; keep retrying in background
//         var options = ConfigurationOptions.Parse(connectionString);
//         options.AbortOnConnectFail = false;
//         var mux = ConnectionMultiplexer.Connect(options);
//         _cache = mux.GetDatabase();
//     }
//     public async Task<JsonArray?> GetAsync(string key)
//     {
//         var value = await _cache.StringGetAsync(key);
//         if (value.IsNullOrEmpty)
//             return null;

//         try
//         {
//             return JsonNode.Parse(value)?.AsArray();
//         }
//         catch
//         {
//             return null; // handle parse errors gracefully
//         }
//     }

//     public async Task SetAsync(string key, JsonArray value, TimeSpan? expiry = null)
//     {
//         var json = value.ToJsonString();
//         await _cache.StringSetAsync(key, json, expiry);
//     }
// }

public class SqliteCacheService : ICacheService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SqliteCacheService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<JsonArray?> GetAsync(string key)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var entry = await context.CacheEntries.FindAsync(key);

        if (entry == null)
            return null;

        //check if expired
        if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
        {
            context.CacheEntries.Remove(entry);
            await context.SaveChangesAsync();
            return null;
        }

        try
        {
            return JsonNode.Parse(entry.Value)?.AsArray();
        }
        catch
        {
            //handle parse errors gracefully
            return null;
        }
    }

    public async Task SetAsync(string key, JsonArray value, TimeSpan? expiry = null)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var json = value.ToJsonString();
        var expiresAt = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : (DateTime?)null;

        var existingEntry = await context.CacheEntries.FindAsync(key);
        if (existingEntry != null)
        {
            existingEntry.Value = json;
            existingEntry.ExpiresAt = expiresAt;
            existingEntry.CreatedAt = DateTime.UtcNow;
        }
        else
        {
            context.CacheEntries.Add(new CacheEntry
            {
                Key = key,
                Value = json,
                ExpiresAt = expiresAt,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string key)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var entry = await context.CacheEntries.FindAsync(key);
        if (entry != null)
        {
            context.CacheEntries.Remove(entry);
            await context.SaveChangesAsync();
        }
    }
}