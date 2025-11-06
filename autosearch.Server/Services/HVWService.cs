using System.Text.Json.Nodes;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using AngleSharp.Text;
using autosearch.Utils;
namespace autosearch.Services;

public class HVWService : ICarService
{
    private readonly ICacheService _cache;

    public HVWService(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<JsonArray> GetAsync()
    {
        var cacheKey = "cars:hvw";
        var cacheValue = await _cache.GetAsync(cacheKey);
        if (cacheValue != null)
        {
            return cacheValue;
        }

        var uri = "https://www.trademe.co.nz/Browse/Motors/DealerShowroom.aspx?sort_order=mtr_price_asc&showroom_id=1296&did=3629";
        var document = await JsRendering.GetRenderedDocumentAsync(uri);
        JsonArray data = HVWParser.Invoke(document);
        //background task
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1 * 60)));
        return data;
    }

    public async Task RevalidateAsync()
    {
        var cacheKey = "cars:hvw";
        //invalidate cache
        await _cache.DeleteAsync(cacheKey);
        
        //rescrape data
        var uri = "https://www.trademe.co.nz/Browse/Motors/DealerShowroom.aspx?sort_order=mtr_price_asc&showroom_id=1296&did=3629";
        var document = await JsRendering.GetRenderedDocumentAsync(uri);
        JsonArray data = HVWParser.Invoke(document);
        
        //background task to update cache
        _ = Task.Run(async () => await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(1 * 60)));
    }
}

public static class HVWParser
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
        var searchResultsContainer = document.QuerySelector("#ListViewList");
        return searchResultsContainer;
    }
    /// <summary>
    /// get search item divs from container
    /// </summary>
    /// <param name="searchResultsContainer"></param>
    /// <returns></returns>
    static IElement[] SearchResultItems(IElement searchResultsContainer)
    {
        return searchResultsContainer.QuerySelectorAll(".listingCard")
            .Where(e => !e.ClassList.Contains("ad-card")) //execude ads
            .Select(e=>e.FirstElementChild)
            .Cast<IElement>()
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
        res["title"] = searchResultItem.QuerySelector(".titleCol")?.QuerySelector(".listingTitle")?.TextContent.StripLeadingTrailingSpaces();
        // res["price"] = searchResultItem.QuerySelector("p.price")?.TextContent;
        // res["location"] = searchResultItem.QuerySelector("p.location")?.TextContent;

        // Get price from the buy now/classify price anchor under .rightColumnPrice
        var priceContainer = searchResultItem.QuerySelector(".rightColumnPrice .listingPrice .listingPrice");
        res["price"] = priceContainer?.TextContent?.Trim() ?? "";

        // Get location from the div with class rightArrows (should be the one after .listingSpecs), or the one with id ending listingLocation
        var locationContainer = searchResultItem.QuerySelector(".titleCol #ListView_CardRepeater_ctl12_card_ctl00_listingLocation")
                                ?? searchResultItem.QuerySelector(".titleCol .rightArrows:last-of-type");
        res["location"] = locationContainer?.TextContent?.Trim() ?? "";

        // Get images from the .listingimageCol img elements
        var images = searchResultItem
            .QuerySelectorAll(".listingimageCol img")
            .Select(img => img.GetAttribute("src"))
            .Where(src => !string.IsNullOrEmpty(src))
            .ToArray();

        var imageNodes = images.Select(src => JsonValue.Create(src)).ToArray();
        res["images"] = new JsonArray(imageNodes);

        //extract url from anchor tag
        var titleElement = searchResultItem.QuerySelector(".titleCol .listingTitle");
        var url = "";
        if (titleElement != null)
        {
            //check if title is an anchor or is inside an anchor
            IElement? anchor = null;
            if (titleElement.TagName == "A")
            {
                anchor = titleElement;
            }
            else
            {
                //check for child anchor
                anchor = titleElement.QuerySelector("a");
                //traverse up parent chain to find anchor
                if (anchor == null)
                {
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
                }
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
