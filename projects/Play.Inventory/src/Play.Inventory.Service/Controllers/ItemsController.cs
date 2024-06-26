using System;
using Microsoft.AspNetCore.Mvc;
using Play.Inventory.Service.Entities;
using System.Threading.Tasks;
using MassTransit.Initializers;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using Play.Common;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _inventoryItemsRepository;
    private readonly IRepository<CatalogItem> _catalogItemsRepository;

    public ItemsController(IRepository<InventoryItem> itemsRepository, IRepository<CatalogItem> catalogItemsRepository)
    {
        _inventoryItemsRepository = itemsRepository;
        _catalogItemsRepository = catalogItemsRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var userInventoryItemEntities = await _inventoryItemsRepository.GetAllAsync(item => item.UserId == userId);

        if (userInventoryItemEntities is null)
        {
            return Ok(new List<InventoryItemDto>());
        }

        var userRelatedInventoryItemsCategoryIds = userInventoryItemEntities.Select(inventoryItem => inventoryItem.CatalogItemId).Distinct().ToList();

        var catalogItemEntities = await _catalogItemsRepository.GetAllAsync(catalogItem => userRelatedInventoryItemsCategoryIds.Contains(catalogItem.Id));

        var userInventoryItemDtos = userInventoryItemEntities.Select(inventoryItem =>
        {
            var catalogItem = catalogItemEntities.Single(catalogItem => catalogItem.Id == inventoryItem.CatalogItemId);

            return inventoryItem.AsDto(catalogItem.Name, catalogItem.Description);
        });

        return Ok(userInventoryItemDtos);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await _inventoryItemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem
            {
                UserId = grantItemsDto.UserId,
                CatalogItemId = grantItemsDto.CatalogItemId,
                Quantity = grantItemsDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await _inventoryItemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDto.Quantity;
            await _inventoryItemsRepository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }

}
