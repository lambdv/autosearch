using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using autosearch.Services;
using Microsoft.Extensions.Caching.Memory;

namespace autosearch.Controllers;
[ApiController]
[Route("cars")]
public class CarsController : ControllerBase
{
    private readonly ILogger<CarsController> _logger;
    //private readonly ICarService _ICarService;
    private readonly ICarService _trademe;
    private readonly ICarService _hvw;
    private readonly ICarService _tic;


    public CarsController(
        ILogger<CarsController> logger,
        ICacheService cache
    )
    {
        _logger = logger;
        _trademe = new TradeMeService(cache);
        _hvw = new HVWService(cache);
        _tic = new TradeInClearanceCarsService(cache);
    }

    [HttpGet("trademe")]
    public async Task<ActionResult<JsonArray>> GetTradeMe()
    {
        try
        {
            JsonArray cars = await _trademe.GetAsync();
            //_logger.LogInformation("Cars: {Cars}", cars.ToJsonString());
            return Ok(cars);
        }
        catch (Exception e)
        {
            //_logger.LogError("Error: {Error}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("hvw")]
    public async Task<ActionResult<JsonArray>> GetHuttValleyWholesalers()
    {
        try
        {
            JsonArray cars = await _hvw.GetAsync();
            //_logger.LogInformation("Cars: {Cars}", cars.ToJsonString());
            return Ok(cars);
        }
        catch (Exception e)
        {
            //_logger.LogError("Error: {Error}", e.Message);
            return BadRequest(e.Message);
        }
    }

    [HttpGet("tic")]
    public async Task<ActionResult<JsonArray>> GetTradeInClearence()
    {
        try
        {
            JsonArray cars = await _tic.GetAsync();
            //_logger.LogInformation("Cars: {Cars}", cars.ToJsonString());
            return Ok(cars);
        }
        catch (Exception e)
        {
            //_logger.LogError("Error: {Error}", e.Message);
            return BadRequest(e.Message);
        }
    }
}
