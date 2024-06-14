using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Extensions;
using Play.Common.Interfaces;
using Play.Common.MassTransit;
using Play.Catalog.Contracts;
using Microsoft.AspNetCore.Authorization;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> _itemsRepository;
    public IPublishEndpoint _publishEndpoint { get; }

    public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
        _itemsRepository = itemsRepository;
    }

    // GET /items
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemDto>>> GetAsync()
    {
        var items = (await _itemsRepository.GetAllAsync()).Select(item => item.AsDto());

        return Ok(items);
    }

    // GET /items/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = (await _itemsRepository.GetAsync(id))?.AsDto();

        return item is not null ? item : NotFound();
    }

    // POST /items
    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto dto)
    {
        var itemEntity = new Item
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CreatedOn = DateTimeOffset.UtcNow
        };

        await _itemsRepository.CreateAsync(itemEntity);

        await _publishEndpoint.Publish(new CatalogItemCreated(itemEntity.Id, itemEntity.Name, itemEntity.Description));

        return CreatedAtAction(nameof(GetByIdAsync), new { id = itemEntity.Id }, itemEntity);
    }

    // PUT /items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto dto)
    {
        var existingItemEntity = await _itemsRepository.GetAsync(id);

        if (existingItemEntity is null)
        {
            return NotFound();
        }

        existingItemEntity.Name = dto.Name;
        existingItemEntity.Description = dto.Description;
        existingItemEntity.Price = dto.Price;

        await _itemsRepository.UpdateAsync(existingItemEntity);

        await _publishEndpoint.Publish(new CatalogItemUpdated(existingItemEntity.Id, existingItemEntity.Name, existingItemEntity.Description));

        return NoContent();
    }

    // DELETE /items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var existingItemEntity = await _itemsRepository.GetAsync(id);

        if (existingItemEntity is null)
        {
            return NotFound();
        }

        await _itemsRepository.RemoveAsync(id);

        await _publishEndpoint.Publish(new CatalogItemDeleted(existingItemEntity.Id));

        return NoContent();
    }
}
