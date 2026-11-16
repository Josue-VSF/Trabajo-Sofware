using Microsoft.AspNetCore.Mvc;
using SgikmMvp1.Api.Contracts;
using SgikmMvp1.Api.Services;

namespace SgikmMvp1.Api.Controllers;

[ApiController]
[Route("api/demo")]
public sealed class DemoController(InventoryStore store) : ControllerBase
{
    [HttpPost("restablecer")]
    public ActionResult<StockResponse> Restablecer()
    {
        return Ok(store.Reset());
    }
}
