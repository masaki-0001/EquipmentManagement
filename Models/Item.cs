namespace EquipmentManagement.Models;

public class Item
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ManagementNumber { get; set; } = string.Empty;

    public DateTime PurchaseDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public decimal? PurchasePrice { get; set; }

    public DateTime? WarrantyUntil { get; set; }

    public string? Category { get; set; }

    public string? Location { get; set; }

    public string? AssignedUser { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
