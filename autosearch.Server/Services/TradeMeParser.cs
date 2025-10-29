using System.Text.Json.Nodes;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;

namespace autosearch.Services;
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

    private static IElement[] SearchResultItems(IElement searchResultsContainer)
    {
        return searchResultsContainer.QuerySelectorAll("tg-col")
            .Where(e => !e.ClassList.Contains("ad-card")) //execude ads
            .ToArray();
    }
    
    private static JsonObject SearchResultItem(IElement searchResultItem)
    {
        //Console.WriteLine(searchResultItem.ToHtml());
        var res = new JsonObject();
        res["title"] = searchResultItem.QuerySelector(".tm-motors-search-card-title__title")?.TextContent;
        // res["price"] = searchResultItem.QuerySelector("p.price")?.TextContent;
        // res["location"] = searchResultItem.QuerySelector("p.location")?.TextContent;

        var priceContainer = searchResultItem.QuerySelector(".tm-search-card-price__price");

        res["price"] = priceContainer?.TextContent ?? "";


        var images = searchResultItem.QuerySelectorAll("img")
            .Select(img => img.GetAttribute("srcset"))
            .Where(srcset => srcset != null)
            .ToArray();

        var imageNodes = images.Select(src => JsonValue.Create(src)).ToArray();
        res["images"] = new JsonArray(imageNodes);

        return res;
    }
}
