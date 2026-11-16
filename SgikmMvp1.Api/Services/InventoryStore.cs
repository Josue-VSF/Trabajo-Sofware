using System.Globalization;
using System.Text;
using SgikmMvp1.Api.Contracts;
using SgikmMvp1.Api.Models;

namespace SgikmMvp1.Api.Services;

public sealed class InventoryStore
{
    private static readonly IReadOnlyDictionary<string, string> Locales =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["A"] = "Local A (Miraflores)",
            ["B"] = "Local B (Pueblo Libre)"
        };

    private readonly object _gate = new();
    private readonly List<InventoryItem> _items = CreateInitialCatalog();
    private readonly List<PedidoDto> _pedidos = [];
    private int _secuencia = 1;

    public IReadOnlyList<LocalDto> GetLocales()
    {
        return Locales
            .Select(entry => new LocalDto(entry.Key, entry.Value))
            .ToArray();
    }

    public StockResponse GetStock(string? buscar)
    {
        lock (_gate)
        {
            return BuildStock(buscar);
        }
    }

    public IReadOnlyList<PedidoDto> GetPedidos()
    {
        lock (_gate)
        {
            return _pedidos.ToArray();
        }
    }

    public PedidoDto? GetPedido(string codigo)
    {
        lock (_gate)
        {
            return _pedidos.FirstOrDefault(
                pedido => string.Equals(pedido.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        }
    }

    public OrderOperationResult CreateOrder(CrearPedidoRequest? request)
    {
        if (request is null)
        {
            return OrderOperationResult.Fail(
                StatusCodes.Status400BadRequest,
                "SOLICITUD_INVALIDA",
                "La solicitud no es válida.");
        }

        lock (_gate)
        {
            var localId = request.LocalId?.Trim();
            if (string.IsNullOrWhiteSpace(localId))
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "LOCAL_REQUERIDO",
                    "Seleccione un local antes de enviar el pedido.");
            }

            if (!Locales.TryGetValue(localId, out var localNombre))
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "LOCAL_INVALIDO",
                    "El local seleccionado no es válido.");
            }

            var insumoId = request.InsumoId?.Trim();
            if (string.IsNullOrWhiteSpace(insumoId))
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "INSUMO_REQUERIDO",
                    "Seleccione un insumo del catálogo.");
            }

            var item = _items.FirstOrDefault(
                entry => string.Equals(entry.Id, insumoId, StringComparison.OrdinalIgnoreCase));

            if (item is null)
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "INSUMO_NO_REGISTRADO",
                    "Insumo no registrado en el catálogo.");
            }

            if (request.Cantidad is null)
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "CANTIDAD_INVALIDA",
                    "Ingrese una cantidad entera mayor a 0.");
            }

            var cantidadSolicitada = request.Cantidad.Value;
            if (cantidadSolicitada <= 0 ||
                decimal.Truncate(cantidadSolicitada) != cantidadSolicitada ||
                cantidadSolicitada > int.MaxValue)
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status400BadRequest,
                    "CANTIDAD_INVALIDA",
                    "La cantidad debe ser un número entero mayor a 0.");
            }

            var cantidad = (int)cantidadSolicitada;
            if (cantidad > item.Disponible)
            {
                return OrderOperationResult.Fail(
                    StatusCodes.Status409Conflict,
                    "STOCK_INSUFICIENTE",
                    $"Stock insuficiente. Máximo disponible: {item.Disponible} {item.Unidad}",
                    item.Disponible);
            }

            item.Reservado += cantidad;

            var ahora = DateTimeOffset.Now;
            var codigo = $"PED-{ahora.Year}-{_secuencia:D3}";
            _secuencia++;

            var pedido = new PedidoDto(
                codigo,
                localId.ToUpperInvariant(),
                localNombre,
                item.Id,
                item.Nombre,
                cantidad,
                item.Unidad,
                ahora,
                "Enviado");

            _pedidos.Insert(0, pedido);

            return OrderOperationResult.Success(pedido, item.Disponible);
        }
    }

    public StockResponse Reset()
    {
        lock (_gate)
        {
            foreach (var item in _items)
            {
                item.Reservado = 0;
            }

            _pedidos.Clear();
            _secuencia = 1;
            return BuildStock(null);
        }
    }

    private StockResponse BuildStock(string? buscar)
    {
        var catalogo = _items.Select(MapStock).ToArray();
        var filtro = Normalize(buscar ?? string.Empty);
        var items = filtro.Length == 0
            ? catalogo
            : catalogo.Where(item => Normalize(item.Nombre).Contains(filtro)).ToArray();

        return new StockResponse(
            items,
            catalogo,
            catalogo.Length,
            catalogo.Count(item => item.Disponible > 0),
            catalogo.Count(item => item.Disponible <= 0));
    }

    private static StockItemDto MapStock(InventoryItem item)
    {
        return new StockItemDto(
            item.Id,
            item.Nombre,
            item.Unidad,
            item.SaldoFisico,
            item.Reservado,
            item.Disponible);
    }

    private static List<InventoryItem> CreateInitialCatalog()
    {
        return
        [
            new InventoryItem { Id = "pan", Nombre = "Pan", Unidad = "unid.", SaldoFisico = 100 },
            new InventoryItem { Id = "chicharron", Nombre = "Chicharrón", Unidad = "kg", SaldoFisico = 10 },
            new InventoryItem { Id = "camote", Nombre = "Camote", Unidad = "kg", SaldoFisico = 0 }
        ];
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString().Trim();
    }
}

public sealed record OrderOperationResult(
    bool EsExitoso,
    PedidoDto? Pedido,
    int DisponibleRestante,
    ApiError? Error,
    int CodigoEstado)
{
    public static OrderOperationResult Success(PedidoDto pedido, int disponibleRestante)
    {
        return new OrderOperationResult(true, pedido, disponibleRestante, null, StatusCodes.Status201Created);
    }

    public static OrderOperationResult Fail(int codigoEstado, string codigo, string mensaje, int? maximoDisponible = null)
    {
        return new OrderOperationResult(
            false,
            null,
            0,
            new ApiError(codigo, mensaje, maximoDisponible),
            codigoEstado);
    }
}
