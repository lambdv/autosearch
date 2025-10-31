using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;

namespace autosearch.Services;

public interface ICacheService
{
    Task SetAsync(string key, JsonArray value, TimeSpan? expiry = null);
    Task<JsonArray?> GetAsync(string key);
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