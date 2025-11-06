using System.Net.Http.Headers;
using FluentAssertions;
using NUnit.Framework;
using System.Text.Json;
using AngleSharp;
using AngleSharp.Dom;
using autosearch.Services;

namespace autosearch.Tests;

public class ParserTest
{
    [Test]
    [Description("Parse TradeMe search results")]
    [Category("Integration")]
    public async Task ParseTradeMeSearchResults()
    {
        //load sample html
        // var URI = "https://www.trademe.co.nz/a/motors/cars/search?vehicle_condition=used&price_max=7500&sort_order=motorsnewestvehicle&user_region=12";
        // var document = await JsRendering.GetRenderedDocumentAsync(URI);

        // var config = AngleSharp.Configuration.Default;
        // var context = AngleSharp.BrowsingContext.New(config);
        // // var document = context.OpenAsync(req => req.Content(html)).Result;

        // // var results = TradeMeParser.Parse(document);
        // Assert.That(document, Is.Not.Null);
        // Assert.That(document.ToHtml(), Is.Not.Null);
        // Assert.That(document.ToHtml(), Is.Not.Empty);
        // Assert.That(document.ToHtml(), Is.Not.Empty);
    }
    [Test]
    [Description("Parse TradeMe search results")]
    public async Task TestTradeInClearanceParser()
    {
        
    }


}
