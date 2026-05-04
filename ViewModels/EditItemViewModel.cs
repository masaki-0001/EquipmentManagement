using System.ComponentModel.DataAnnotations;

namespace EquipmentManagement.ViewModels;

public class EditItemViewModel
{
    public int Id { get; set; }

    public string ManagementNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "名称は必須です。")]
    [StringLength(100, ErrorMessage = "名称は100文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "購入日は必須です。")]
    public DateTime PurchaseDate { get; set; }

    [Required(ErrorMessage = "状態は必須です。")]
    [StringLength(20, ErrorMessage = "状態は20文字以内で入力してください。")]
    public string Status { get; set; } = string.Empty;
}
