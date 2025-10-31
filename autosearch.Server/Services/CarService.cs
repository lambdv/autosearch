using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Threading.Tasks;


namespace autosearch.Services;

public interface ICarService
{
    Task<JsonArray> GetAsync();
    static HttpClient Client
    {
        get {
            var client = new HttpClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
            return client;
        }
    }
}
public class TradeMeService : ICarService
{
    private readonly ICacheService _cache;

    public TradeMeService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<JsonArray> GetAsync()
    {
        var cacheKey = "cars:trademe";
        var cacheValue = await _cache.GetAsync(cacheKey);
        if (cacheValue != null)
        {
            return cacheValue;
        }

        var uri = "https://www.trademe.co.nz/a/motors/cars/search?vehicle_condition=used&price_max=7500&sort_order=motorsnewestvehicle&user_region=12";
        var document = await JsRendering.GetRenderedDocumentAsync(uri);
        JsonArray data = TradeMeParser.Invoke(document);
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1*60)));
        return data;
    }
}

public class CoventryCarsService : ICarService
{
    public async Task<JsonArray> GetAsync()
    {
        throw new NotImplementedException();
    }
}

public class TwoCheapCarsService : ICarService
{
    public async Task<JsonArray> GetAsync()
    {
       throw new NotImplementedException();
    }
}