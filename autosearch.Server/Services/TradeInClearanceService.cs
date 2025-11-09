using System.Text.Json.Nodes;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using autosearch.Utils;

namespace autosearch.Services;


public class TradeInClearanceCarsService : ICarService
{
    
    private readonly ICacheService _cache;

    public TradeInClearanceCarsService(ICacheService cache)
    {
        _cache = cache;
    }
    public async Task<JsonArray> GetAsync()
    {
        var cacheKey = "cars:tradeinclearance";
        var cacheValue = await _cache.GetAsync(cacheKey);
        if (cacheValue != null)
        {
            return cacheValue;
        }
        var uri = "https://www.tradeinclearance.co.nz/vehicles";
        var document = await JsRendering.GetRenderedDocumentAsync(uri);

        JsonArray data = TradeInClearanceCarsParser.Invoke(document);
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1 * 60)));
        return data;
    }

    public async Task RevalidateAsync()
    {
        var cacheKey = "cars:tradeinclearance";
        //invalidate cache
        await _cache.DeleteAsync(cacheKey);
        
        //rescrape data
        var uri = "https://www.tradeinclearance.co.nz/vehicles";
        var document = await JsRendering.GetRenderedDocumentAsync(uri);
        JsonArray data = TradeInClearanceCarsParser.Invoke(document);
        
        //background task to update cache
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1 * 60)));
    }
}

public static class TradeInClearanceCarsParser
{
    public static JsonArray Invoke(IDocument document)
    {
        var res = new JsonArray();
        var resultContainer = document.QuerySelector(".vehicle-results");
        var resultUl = document.QuerySelector(".vehicle-list");
        var lis = resultUl.QuerySelectorAll(".vehicle");

        foreach (var li in lis)
        {
            res.Add(parseItem(li));
        }
        return res;
    }

    private static JsonObject parseItem(IElement li)
    {
        var res = new JsonObject();
        var info = li.QuerySelector("vehicle-info");
        if (info == null)
        {
            throw new Exception("failed to find div with vehicle-info in .vehicle li item");
        }
        res["title"] = info.FirstChild?.TextContent;
        //extract url from anchor tag
        var link = li.QuerySelector("a");
        var url = link?.GetAttribute("href") ?? "";
        //make absolute url if relative
        if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
        {
            url = "https://www.tradeinclearance.co.nz" + (url.StartsWith("/") ? "" : "/") + url;
        }
        res["url"] = url;
        return res;
    }

}


