using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;

namespace Play.Catalog.Service.Extensions;

public static class Extensions
{
    public static ItemDto AsDto(this Item item)
    {
        if (item is null) return null;

        return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedOn);
    }
}
