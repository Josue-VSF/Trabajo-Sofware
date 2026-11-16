using Microsoft.AspNetCore.Mvc;
using SgikmMvp1.Api.Contracts;
using SgikmMvp1.Api.Services;

namespace SgikmMvp1.Api.Controllers;

[ApiController]
[Route("api/locales")]
public sealed class LocalesController(InventoryStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<LocalDto>> Listar()
    {
        return Ok(store.GetLocales());
    }
}
