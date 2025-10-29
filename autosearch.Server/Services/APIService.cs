using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace autosearch.Services;

public abstract class CarService
{
    public abstract Task<JsonArray> GetAsync();
    static readonly HttpClient Client = new HttpClient();

    static CarService(){
        Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36");
        Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8");
        Client.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
    }
}
public class TradeMeService : CarService
{
    public override async Task<JsonArray> GetAsync()
    {
        var URI = "https://www.trademe.co.nz/a/motors/cars/search?vehicle_condition=used&price_max=7500&sort_order=motorsnewestvehicle&user_region=12";
        var document = await JsRendering.GetRenderedDocumentAsync(URI);
        JsonArray data = TradeMeParser.Parse(document);
        return data;
    }
}

//https://www.wholesalecarsdirect.co.nz/
//https://www.mikebakermotors.co.nz/
//https://www.autoworld.co.nz/
//https://www.coventrycars.co.nz/
//https://www.2cheapcars.co.nz/dealership/wellington?utm_source=google-my-business&utm_medium=organic&utm_content=gmb-lower%20hutt


