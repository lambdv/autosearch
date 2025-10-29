using Microsoft.Playwright;
using AngleSharp;
using AngleSharp.Dom;

namespace autosearch.Services;
public static class JsRendering
{
	public static async Task<IDocument> GetRenderedDocumentAsync(string url)
	{
		using var playwright = await Playwright.CreateAsync();
		await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = true
		});

		var context = await browser.NewContextAsync(new BrowserNewContextOptions
		{
			UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36", //pretend to be a real browser
			Locale = "en-US",
			TimezoneId = "Pacific/Auckland" //helps some nz sites
		});

		var page = await context.NewPageAsync();
		await page.GotoAsync(url, new PageGotoOptions
		{
			//use domcontentloaded to avoid waiting forever on pages with long-running network activity
			WaitUntil = WaitUntilState.DOMContentLoaded,
			Timeout = 15000
		});

		//give the app a moment to fetch and render listings
		try {
			await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 }); 
		} catch {}

		try
		{
			//wait briefly for results container but continue if it never appears
			await page.WaitForSelectorAsync(".tm-motors-search-results__results-container", new PageWaitForSelectorOptions
			{
				Timeout = 10000
			});
		}
		catch
		{}

		//wait for at least one real listing title to appear (not loading skeleton)
		try
		{
			await page.WaitForFunctionAsync(
				"() => Array.from(document.querySelectorAll('.tm-motors-search-card-title__title'))\n\t\t\t\t\t\t.some(e => e.textContent && e.textContent.trim().length > 0)",
				new PageWaitForFunctionOptions { Timeout = 10000 }
			);
		}
		catch
		{}
		var html = await page.ContentAsync();

		var angleContext = BrowsingContext.New(Configuration.Default);
		return await angleContext.OpenAsync(req => req.Content(html));
	}
}