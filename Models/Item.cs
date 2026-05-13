namespace EquipmentManagement.Models;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ManagementNumber { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }
}
