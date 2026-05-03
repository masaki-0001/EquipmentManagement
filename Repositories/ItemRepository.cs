using EquipmentManagement.Models;

namespace EquipmentManagement.Repositories;

public static class ItemRepository
{
    private static readonly List<Item> Items = new()
    {
        new Item
        {
            Id = 1,
            Name = "ノートPC",
            ManagementNumber = "ITEM-0001",
            PurchaseDate = new DateTime(2026, 5, 2),
            Status = "使用中"
        },
        new Item
        {
            Id = 2,
            Name = "プロジェクター",
            ManagementNumber = "ITEM-0002",
            PurchaseDate = new DateTime(2026, 5, 1),
            Status = "保管中"
        }
    };

    public static List<Item> GetAll()
    {
        return Items
            .Where(x => !x.IsDeleted)
            .ToList();
    }

    public static Item? GetById(int id)
    {
        return Items.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
    }

    public static void Add(Item item)
    {
        var nextId = Items.Any() ? Items.Max(x => x.Id) + 1 : 1;

        item.Id = nextId;
        item.ManagementNumber = $"ITEM-{item.Id:0000}";
        item.IsDeleted = false;

        Items.Add(item);
    }

    public static void Update(Item item)
    {
        var existingItem = GetById(item.Id);

        if (existingItem is null)
        {
            return;
        }

        existingItem.Name = item.Name;
        existingItem.PurchaseDate = item.PurchaseDate;
        existingItem.Status = item.Status;
    }

    public static void Delete(int id)
    {
        var item = Items.FirstOrDefault(x => x.Id == id);

        if (item is not null)
        {
            item.IsDeleted = true;
        }
    }

    public static bool ExistsManagementNumber(string managementNumber, int? excludeId = null)
    {
        return Items.Any(x =>
            x.ManagementNumber == managementNumber &&
            x.Id != excludeId);
    }

    public static List<Item> Search(string? keyword)
    {
        var query = Items.Where(x => !x.IsDeleted);

        if (string.IsNullOrWhiteSpace(keyword))
        {
            return query.ToList();
        }

        return query
            .Where(x =>
                x.Name.Contains(keyword) ||
                x.ManagementNumber.Contains(keyword) ||
                x.Status.Contains(keyword))
            .ToList();
    }
}