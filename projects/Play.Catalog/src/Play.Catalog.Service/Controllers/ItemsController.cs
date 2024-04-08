using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;

namespace Play.Catalog.Service.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemsController : ControllerBase
{
    public static readonly List<ItemDto> items =
    [
        new ItemDto(Guid.NewGuid(), "Potion", "Restores a small amount of HP", 5, DateTimeOffset.UtcNow),
        new ItemDto(Guid.NewGuid(), "Antidote", "Cures poison", 7, DateTimeOffset.UtcNow),
        new ItemDto(Guid.NewGuid(), "Bronze sword", "Deals a small amount of damage", 20, DateTimeOffset.UtcNow)
    ];

    // GET /items
    [HttpGet]
    public IEnumerable<ItemDto> Get() => items;

    // GET /items/{id}
    [HttpGet("{id}")]
    public ItemDto GetById(Guid id) => items.SingleOrDefault(item => item.Id == id);

    // POST /items
    [HttpPost]
    public ActionResult<ItemDto> Post(CreateItemDto dto)
    {
        var item = new ItemDto(Guid.NewGuid(), dto.Name, dto.Description, dto.Price, DateTimeOffset.UtcNow);
        items.Add(item);

        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    // PUT /items/{id}
    [HttpPut("{id}")]
    public IActionResult Put(Guid id, UpdateItemDto dto)
    {
        var existingItem = items.SingleOrDefault(existingItem => existingItem.Id == id);

        var updatedItem = existingItem with
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price
        };

        var index = items.FindIndex(existingItem => existingItem.Id == id);

        items[index] = updatedItem;

        return NoContent();
    }

    // DELETE /items/{id}
    [HttpDelete("{id}")]
    public IActionResult Delete(Guid id)
    {
        var index = items.FindIndex(existingItem => existingItem.Id == id);

        items.RemoveAt(index);

        return NoContent();
    }
}
