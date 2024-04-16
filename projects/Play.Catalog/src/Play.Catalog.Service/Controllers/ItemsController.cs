using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Catalog.Service.Extensions;
using Play.Catalog.Service.Repositories;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{

    private readonly ItemsRepository itemsRepository = new();



    // GET /items
    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAsync()
    {
        var items = (await itemsRepository.GetAllAsync()).Select(item => item.AsDto());

        return items;
    }

    // GET /items/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = (await itemsRepository.GetAsync(id))?.AsDto();

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

        await itemsRepository.CreateAsync(itemEntity);

        return CreatedAtAction(nameof(GetByIdAsync), new { id = itemEntity.Id }, itemEntity);
    }

    // PUT /items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto dto)
    {
        var existingItemEntity = await itemsRepository.GetAsync(id);

        if (existingItemEntity is null)
        {
            return NotFound();
        }

        existingItemEntity.Name = dto.Name;
        existingItemEntity.Description = dto.Description;
        existingItemEntity.Price = dto.Price;

        await itemsRepository.UpdateAsync(existingItemEntity);

        return NoContent();
    }

    // DELETE /items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var existingItemEntity = await itemsRepository.GetAsync(id);

        if (existingItemEntity is null)
        {
            return NotFound();
        }

        await itemsRepository.RemoveAsync(id);

        return NoContent();
    }
}
