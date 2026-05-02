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
        return Items;
    }

    public static Item? GetById(int id)
    {
        return Items.FirstOrDefault(x => x.Id == id);
    }

    public static void Add(Item item)
    {
        item.Id = Items.Any() ? Items.Max(x => x.Id) + 1 : 1;
        item.ManagementNumber = $"ITEM-{item.Id:0000}";

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
        var item = GetById(id);

        if (item is not null)
        {
            Items.Remove(item);
        }
    }

    public static bool ExistsManagementNumber(string managementNumber, int? excludeId = null)
    {
        return Items.Any(x =>
            x.ManagementNumber == managementNumber &&
            x.Id != excludeId);
    }
}