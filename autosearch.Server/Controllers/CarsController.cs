using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using autosearch.Services;

namespace autosearch.Controllers;
[ApiController]
[Route("cars")]
public class CarsController : ControllerBase
{
    private readonly ILogger<CarsController> _logger;
    private readonly ICarService _ICarService;
    public CarsController(
        ILogger<CarsController> logger,
        ICarService ICarService)
    {
        _logger = logger;
        _ICarService = ICarService;
    }

    [HttpGet]
    public async Task<ActionResult<JsonArray>> GetTradeMeCars()
    {
        try
        {
            JsonArray cars = await _ICarService.GetAsync();
            _logger.LogInformation("Cars: {Cars}", cars.ToJsonString());
            return Ok(cars);
        }
        catch (Exception e)
        {
            _logger.LogError("Error: {Error}", e.Message);
            return BadRequest(e.Message);
        }
    }
}
