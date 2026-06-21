using System.ComponentModel.DataAnnotations;

namespace EquipmentManagement.ViewModels;

public class CreateItemViewModel
{
    [Required(ErrorMessage = "備品名は必須です")]
    [StringLength(100, ErrorMessage = "備品名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "購入日は必須です")]
    public DateTime PurchaseDate { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "購入金額は0円以上999,999,999円以下で入力してください")]
    public decimal? PurchasePrice { get; set; }

    [StringLength(50, ErrorMessage = "カテゴリは50文字以内で入力してください")]
    public string? Category { get; set; }

    [StringLength(50, ErrorMessage = "保管場所は50文字以内で入力してください")]
    public string? Location { get; set; }

    [StringLength(50, ErrorMessage = "使用者は50文字以内で入力してください")]
    public string? AssignedUser { get; set; }

    [Required(ErrorMessage = "状態は必須です")]
    public string Status { get; set; } = string.Empty;
}