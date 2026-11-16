using Microsoft.AspNetCore.Mvc;
using SgikmMvp1.Api.Contracts;
using SgikmMvp1.Api.Services;

namespace SgikmMvp1.Api.Controllers;

[ApiController]
[Route("api/almacen")]
public sealed class AlmacenController(InventoryStore store) : ControllerBase
{
    [HttpGet("stock")]
    public ActionResult<StockResponse> ConsultarStock([FromQuery] string? buscar)
    {
        return Ok(store.GetStock(buscar));
    }
}
