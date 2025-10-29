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
    public void ParseTradeMeSearchResults()
    {
        //load sample html
        var html = File.ReadAllText("./resources/trademeResultSample.html");
        var config = AngleSharp.Configuration.Default;
        var context = AngleSharp.BrowsingContext.New(config);
        var document = context.OpenAsync(req => req.Content(html)).Result;

        var results = TradeMeParser.Parse(document);
    }

}
