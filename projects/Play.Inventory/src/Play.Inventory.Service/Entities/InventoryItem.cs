using System;
using Play.Common.Interfaces;

namespace Play.Inventory.Service.Entities;

public class InventoryItem : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; init; }
    public Guid CatalogItemId { get; init; }
    public int Quantity { get;  set; }
    public DateTimeOffset AcquiredDate { get; init; }
}
