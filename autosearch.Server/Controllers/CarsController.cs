using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using autosearch.Services;

namespace autosearch.Controllers;
[ApiController]
[Route("cars")]
public class CarsController : ControllerBase
{
    private readonly ILogger<CarsController> _logger;
    private readonly CarService _carService;
    public CarsController(
        ILogger<CarsController> logger,
        CarService carService)
    {
        _logger = logger;
        _carService = carService;
    }

    [HttpGet]
    public async Task<ActionResult<JsonArray>> GetTradeMeCars()
    {
        JsonArray cars = await _carService.GetAsync();
        return Ok(cars);
    }
}
