using System.Net.Http.Headers;
using System.Net.Sockets;
using FluentAssertions;
using NUnit.Framework;
using System.Text.Json;
namespace autosearch.Tests;
public static class TestConfig
{
    public static readonly string ApiBaseUrl =
        Environment.GetEnvironmentVariable("API_BASE_URL")?.TrimEnd('/')
        ?? "http://localhost:5030";
}

public class CarsEndpointTests
{
    private static bool IsServerAvailable(Uri baseAddress)
    {
        try
        {
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(baseAddress.Host, baseAddress.Port > 0 ? baseAddress.Port : (baseAddress.Scheme == Uri.UriSchemeHttps ? 443 : 80));
            return connectTask.Wait(TimeSpan.FromSeconds(1));
        }
        catch
        {
            return false;
        }
    }

    private static HttpClient CreateClient()
    {
        var httpClient = new HttpClient
        {
            BaseAddress = new Uri(TestConfig.ApiBaseUrl),
            Timeout = TimeSpan.FromSeconds(60)
        };
        httpClient.DefaultRequestHeaders.Accept.Clear();
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
        return httpClient;
    }

    [Test]
    [Description("GET /cars returns 200 and JSON array")]
    [Category("Integration")]
    public async Task GetCars_ReturnsOkAndJsonArray()
    {
        var baseAddress = new Uri(TestConfig.ApiBaseUrl);
        if (!IsServerAvailable(baseAddress))
        {
            Assert.Ignore($"start the server first: dotnet run --project autosearch.Server (expected at {baseAddress})");
        }

        using var client = CreateClient();
        using var response = await client.GetAsync("/cars");

        response.IsSuccessStatusCode
            .Should()
            .BeTrue(
                $"expected 2xx from {client.BaseAddress}/cars, got {(int)response.StatusCode} {response.ReasonPhrase}. start the server: dotnet run --project autosearch.Server"
            );

        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        json.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
    }
}
