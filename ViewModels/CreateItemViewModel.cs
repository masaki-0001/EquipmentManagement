using System.ComponentModel.DataAnnotations;

namespace EquipmentManagement.ViewModels;

public class CreateItemViewModel
{
    [Required(ErrorMessage = "備品名は必須です")]
    [StringLength(100, ErrorMessage = "備品名は100文字以内で入力してください")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "購入日は必須です")]
    public DateTime PurchaseDate { get; set; }

    [Display(Name = "購入金額")]
    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "購入金額は0円以上999,999,999円以下で入力してください")]
    public decimal? PurchasePrice { get; set; }

    [Display(Name = "保証期限")]
    public DateTime? WarrantyUntil { get; set; }

    [StringLength(50, ErrorMessage = "カテゴリは50文字以内で入力してください")]
    public string? Category { get; set; }

    [StringLength(50, ErrorMessage = "保管場所は50文字以内で入力してください")]
    public string? Location { get; set; }

    [StringLength(50, ErrorMessage = "使用者は50文字以内で入力してください")]
    public string? AssignedUser { get; set; }

    [Required(ErrorMessage = "状態は必須です")]
    public string Status { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "備考は1000文字以内で入力してください")]
    public string? Note { get; set; }

    [Display(Name = "最終確認日")]
    public DateTime? LastConfirmedAt { get; set; }

    [StringLength(1000, ErrorMessage = "確認メモは1000文字以内で入力してください")]
    public string? ConfirmationNote { get; set; }
}
