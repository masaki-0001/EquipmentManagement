using EquipmentManagement.Models;

namespace EquipmentManagement.ViewModels;

public class ItemIndexViewModel
{
    public List<Item> Items { get; set; } = new();

    public string? Keyword { get; set; }
    public string? Category { get; set; }
    public string? Location { get; set; }
    public string? Status { get; set; }
    public string SortOrder { get; set; } = "managementNumber";

    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public int ExpiredWarrantyCount { get; set; }
    public int ExpiringWarrantyCount { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;

    public bool HasWarrantyAlerts =>
        ExpiredWarrantyCount > 0 || ExpiringWarrantyCount > 0;

    public int FirstItemNumber => TotalCount == 0 ? 0 : ((CurrentPage - 1) * PageSize) + 1;
    public int LastItemNumber => Math.Min(CurrentPage * PageSize, TotalCount);
}