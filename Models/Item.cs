using System.ComponentModel.DataAnnotations;

namespace EquipmentManagement.Models;

public class Item
{
    public int Id { get; set; }

    [Required(ErrorMessage = "名称は必須です。")]
    public string Name { get; set; } = string.Empty;

    public string ManagementNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "購入日は必須です。")]
    public DateTime PurchaseDate { get; set; }

    public string Status { get; set; } = string.Empty;
}