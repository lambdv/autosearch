using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using autosearch.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using autosearch.Utils;

namespace autosearch.Services;

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
        //background task
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1 * 60)));
        return data;
    }

    public async Task RevalidateAsync()
    {
        var cacheKey = "cars:trademe";
        //invalidate cache
        await _cache.DeleteAsync(cacheKey);

        //rescrape data
        var uri = "https://www.trademe.co.nz/a/motors/cars/search?vehicle_condition=used&price_max=7500&sort_order=motorsnewestvehicle&user_region=12";
        var document = await JsRendering.GetRenderedDocumentAsync(uri);
        JsonArray data = TradeMeParser.Invoke(document);

        //background task to update cache
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1 * 60)));
    }
}


public static class TradeMeParser
{
    public static JsonArray Invoke(IDocument document)
    {
        var res = new JsonArray();

        // Console.WriteLine(document.ToHtml());

        var searchResultsContainer = SearchResultsContainer(document);
        if (searchResultsContainer == null)
        {
            return res;
        }

        var searchResultItems = SearchResultItems(searchResultsContainer);
        foreach (var searchResultItem in searchResultItems)
        {
            res.Add(SearchResultItem(searchResultItem));
        }
        return res;
    }

    /// <summary>
    /// gets the first div with class tm-motors-search-results__results-container
    /// </summary>
    /// <param name="document"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private static IElement? SearchResultsContainer(AngleSharp.Dom.IDocument document)
    {
        var searchResultsContainer = document.QuerySelector(".tm-motors-search-results__results-container");
        return searchResultsContainer;
    }
    /// <summary>
    /// get search item divs from container
    /// </summary>
    /// <param name="searchResultsContainer"></param>
    /// <returns></returns>
    static IElement[] SearchResultItems(IElement searchResultsContainer)
    {
        return searchResultsContainer.QuerySelectorAll("tg-col")
            .Where(e => !e.ClassList.Contains("ad-card")) //execude ads
            .ToArray();
    }

    /// <summary>
    /// convert search item divs to json
    /// </summary>
    /// <param name="searchResultItem"></param>
    /// <returns></returns>
    static JsonObject SearchResultItem(IElement searchResultItem)
    {
        //Console.WriteLine(searchResultItem.ToHtml());
        var res = new JsonObject();
        res["title"] = searchResultItem.QuerySelector(".tm-motors-search-card-title__title")?.TextContent;

        // Extract price reliably
        var priceContainer = searchResultItem.QuerySelector(".tm-search-card-price__price");
        res["price"] = priceContainer?.TextContent?.Trim() ?? "";

        // --- Handle getting images: both fallback and carousel pattern ---
        var images = new List<string>();

        // 1. Try carousel images: look for all <img> tags inside .tm-progressive-image-loader__responsive-container
        var carouselImgs = searchResultItem.QuerySelectorAll(".tm-progressive-image-loader__responsive-container img");
        foreach (var img in carouselImgs)
        {
            var src = img.GetAttribute("src");
            if (!string.IsNullOrWhiteSpace(src))
            {
                images.Add(src);
            }
        }

        // 2. If nothing, try any <img> children directly, fallback to the old method
        if (images.Count == 0)
        {
            // srcset can appear empty on images without carousel
            var imgElements = searchResultItem.QuerySelectorAll("img");
            foreach (var img in imgElements)
            {
                // Prefer src if present, otherwise fallback to srcset first match
                var src = img.GetAttribute("src");
                var srcset = img.GetAttribute("srcset");

                if (!string.IsNullOrWhiteSpace(src))
                {
                    images.Add(src);
                }
                else if (!string.IsNullOrWhiteSpace(srcset))
                {
                    // srcset could be multiple sizes, we just take the first URL (split by comma, take left token, then split by whitespace)
                    var firstSrcSet = srcset.Split(',')[0].Trim();
                    var imageUrl = firstSrcSet.Split(' ')[0].Trim();
                    if (!string.IsNullOrEmpty(imageUrl))
                        images.Add(imageUrl);
                }
            }
        }

        // Remove dupes and nulls
        var imageNodes = images
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Select(src => JsonValue.Create(src))
            .ToArray();
        res["images"] = new JsonArray(imageNodes);

        //extract url from anchor tag, typically the title link or parent anchor
        var titleElement = searchResultItem.QuerySelector(".tm-motors-search-card-title__title");
        var url = "";
        if (titleElement != null)
        {
            //check if title is inside an anchor or if there's a parent anchor
            IElement? anchor = null;
            if (titleElement.ParentElement?.TagName == "A")
            {
                anchor = titleElement.ParentElement;
            }
            else
            {
                //traverse up parent chain to find anchor
                var parent = titleElement.ParentElement;
                while (parent != null && anchor == null)
                {
                    if (parent.TagName == "A")
                    {
                        anchor = parent;
                        break;
                    }
                    parent = parent.ParentElement;
                }
                //fallback to any anchor in the search result item
                anchor ??= searchResultItem.QuerySelector("a");
            }
            url = anchor?.GetAttribute("href") ?? "";
        }
        //make absolute url if relative
        if (!string.IsNullOrEmpty(url) && !url.StartsWith("http"))
        {
            url = "https://www.trademe.co.nz" + (url.StartsWith("/") ? "" : "/") + url;
        }
        res["url"] = url;

        return res;
    }
}