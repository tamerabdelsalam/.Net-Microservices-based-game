using System;
using Play.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Play.Inventory.Service.Entities;
using System.Threading.Tasks;
using MassTransit.Initializers;
using System.Collections.Generic;
using System.Linq;

namespace Play.Inventory.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<InventoryItem> _itemsRepository;
    public ItemsController(IRepository<InventoryItem> itemsRepository)
    {
        _itemsRepository = itemsRepository;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetAsync(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest();
        }

        var userItems = (await _itemsRepository.GetAllAsync(item => item.UserId == userId)).Select(item => item.AsDto());

        return Ok(userItems);
    }

    [HttpPost]
    public async Task<ActionResult> PostAsync(GrantItemsDto grantItemsDto)
    {
        var inventoryItem = await _itemsRepository.GetAsync(item => item.UserId == grantItemsDto.UserId && item.CatalogItemId == grantItemsDto.CatalogItemId);

        if (inventoryItem is null)
        {
            inventoryItem = new InventoryItem
            {
                UserId = grantItemsDto.UserId,
                CatalogItemId = grantItemsDto.CatalogItemId,
                Quantity = grantItemsDto.Quantity,
                AcquiredDate = DateTimeOffset.UtcNow
            };

            await _itemsRepository.CreateAsync(inventoryItem);
        }
        else
        {
            inventoryItem.Quantity += grantItemsDto.Quantity;
            await _itemsRepository.UpdateAsync(inventoryItem);
        }

        return Ok();
    }

}
