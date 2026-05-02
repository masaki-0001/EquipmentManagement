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
}