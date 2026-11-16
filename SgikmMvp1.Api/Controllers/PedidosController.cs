using Microsoft.AspNetCore.Mvc;
using SgikmMvp1.Api.Contracts;
using SgikmMvp1.Api.Services;

namespace SgikmMvp1.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public sealed class PedidosController(InventoryStore store) : ControllerBase
{
    [HttpGet]
    public ActionResult<IReadOnlyList<PedidoDto>> Listar()
    {
        return Ok(store.GetPedidos());
    }

    [HttpGet("{codigo}")]
    public ActionResult<PedidoDto> Obtener(string codigo)
    {
        var pedido = store.GetPedido(codigo);

        return pedido is null
            ? NotFound(new ApiError("PEDIDO_NO_ENCONTRADO", "El pedido solicitado no existe."))
            : Ok(pedido);
    }

    [HttpPost]
    public ActionResult<CrearPedidoResponse> Crear([FromBody] CrearPedidoRequest? request)
    {
        var result = store.CreateOrder(request);

        if (!result.EsExitoso)
        {
            return StatusCode(result.CodigoEstado, result.Error);
        }

        return CreatedAtAction(
            nameof(Obtener),
            new { codigo = result.Pedido!.Codigo },
            new CrearPedidoResponse(result.Pedido, result.DisponibleRestante));
    }
}
