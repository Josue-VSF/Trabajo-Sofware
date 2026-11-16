namespace SgikmMvp1.Api.Models;

public sealed class InventoryItem
{
    public required string Id { get; init; }
    public required string Nombre { get; init; }
    public required string Unidad { get; init; }
    public int SaldoFisico { get; init; }
    public int Reservado { get; set; }
    public int Disponible => SaldoFisico - Reservado;
}
