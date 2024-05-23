using System;

namespace Play.Inventory.Service;

public record GrantItemsDto(Guid UserId, Guid CatalogItemId, int Quantity); // DTO for the GrantItems method to define the items to be granted for a user 

public record InventoryItemDto(Guid CatalogItemId, string Name, string Description, int Quantity, DateTimeOffset AcquiredDate); // DTO for the InventoryItems

public record CatalogItemDto(Guid Id, string Name, string Description); // DTO for the CatalogItems


