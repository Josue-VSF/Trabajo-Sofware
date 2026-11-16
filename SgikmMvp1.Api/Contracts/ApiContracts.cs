namespace SgikmMvp1.Api.Contracts;

public sealed record LocalDto(string Id, string Nombre);

public sealed record StockItemDto(
    string Id,
    string Nombre,
    string Unidad,
    int SaldoFisico,
    int Reservado,
    int Disponible);

public sealed record StockResponse(
    IReadOnlyList<StockItemDto> Items,
    IReadOnlyList<StockItemDto> Catalogo,
    int TotalRegistrados,
    int Disponibles,
    int Agotados);

public sealed record CrearPedidoRequest(string? LocalId, string? InsumoId, decimal? Cantidad);

public sealed record PedidoDto(
    string Codigo,
    string LocalId,
    string Local,
    string InsumoId,
    string Insumo,
    int Cantidad,
    string Unidad,
    DateTimeOffset Hora,
    string Estado);

public sealed record CrearPedidoResponse(PedidoDto Pedido, int DisponibleRestante);

public sealed record ApiError(string Codigo, string Mensaje, int? MaximoDisponible = null);
