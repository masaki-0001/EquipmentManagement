using System.Text;
using Microsoft.AspNetCore.Mvc;
using EquipmentManagement.Models;
using EquipmentManagement.Repositories;
using EquipmentManagement.ViewModels;

namespace EquipmentManagement.Controllers;

public class ItemsController : Controller
{
    private readonly ItemRepository _itemRepository;

    public ItemsController(ItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    private static readonly string[] ValidStatuses =
    {
        "使用中",
        "保管中",
        "修理中",
        "廃棄済み"
    };

    private static readonly string[] ValidCategories =
    {
        "PC",
        "周辺機器",
        "工具",
        "事務用品",
        "消耗品",
        "その他"
    };

    private static readonly string[] ValidLocations =
    {
        "事務所",
        "倉庫",
        "車両内",
        "現場",
        "その他"
    };

    private static readonly Dictionary<string, string> SortOptions = new()
    {
        { "managementNumber", "管理番号順" },
        { "purchaseDateDesc", "購入日が新しい順" },
        { "updatedAtDesc", "更新日が新しい順" },
        { "warrantyUntilAsc", "保証期限が近い順" }
    };

    private static readonly Dictionary<string, string> ConfirmationFilterOptions = new()
    {
        { "all", "すべて" },
        { "unconfirmed", "未確認" },
        { "over90days", "90日以上未確認" },
        { "over180days", "180日以上未確認" },
        { "over365days", "1年以上未確認" }
    };

    private void SetSelectLists()
    {
        ViewBag.Statuses = ValidStatuses;
        ViewBag.Categories = ValidCategories;
        ViewBag.Locations = ValidLocations;
        ViewBag.SortOptions = SortOptions;
        ViewBag.ConfirmationFilterOptions = ConfirmationFilterOptions;
    }

    private void ValidatePurchasePrice(decimal? purchasePrice, string fieldName)
    {
        if (purchasePrice.HasValue &&
            decimal.Truncate(purchasePrice.Value) != purchasePrice.Value)
        {
            ModelState.AddModelError(fieldName, "購入金額は1円単位の整数で入力してください。");
        }
    }

    private void ValidateWarrantyUntil(DateTime purchaseDate, DateTime? warrantyUntil, string fieldName)
    {
        if (warrantyUntil.HasValue && warrantyUntil.Value.Date < purchaseDate.Date)
        {
            ModelState.AddModelError(fieldName, "保証期限は購入日以降の日付を入力してください。");
        }
    }

    private static string BuildCsv(List<Item> items)
    {
        var builder = new StringBuilder();

        builder.AppendLine(string.Join(",",
            CsvEscape("管理番号"),
            CsvEscape("名称"),
            CsvEscape("カテゴリ"),
            CsvEscape("購入日"),
            CsvEscape("購入金額"),
            CsvEscape("保証期限"),
            CsvEscape("保管場所"),
            CsvEscape("使用者"),
            CsvEscape("状態"),
            CsvEscape("備考"),
            CsvEscape("最終確認日"),
            CsvEscape("確認メモ"),
            CsvEscape("登録日時"),
            CsvEscape("更新日時")));

        foreach (var item in items)
        {
            builder.AppendLine(string.Join(",",
                CsvEscape(item.ManagementNumber),
                CsvEscape(item.Name),
                CsvEscape(item.Category),
                CsvEscape(FormatDate(item.PurchaseDate)),
                CsvEscape(FormatPrice(item.PurchasePrice)),
                CsvEscape(FormatDate(item.WarrantyUntil)),
                CsvEscape(item.Location),
                CsvEscape(item.AssignedUser),
                CsvEscape(item.Status),
                CsvEscape(item.Note),
                CsvEscape(FormatDate(item.LastConfirmedAt)),
                CsvEscape(item.ConfirmationNote),
                CsvEscape(FormatDateTime(item.CreatedAt)),
                CsvEscape(FormatDateTime(item.UpdatedAt))));
        }

        return builder.ToString();
    }

    private static string CsvEscape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var escapedValue = value.Replace("\"", "\"\"");

        if (escapedValue.Contains(',') ||
            escapedValue.Contains('"') ||
            escapedValue.Contains('\r') ||
            escapedValue.Contains('\n'))
        {
            return $"\"{escapedValue}\"";
        }

        return escapedValue;
    }

    private static string FormatDate(DateTime date)
    {
        return date.ToString("yyyy/MM/dd");
    }

    private static string FormatDate(DateTime? date)
    {
        return date.HasValue
            ? date.Value.ToString("yyyy/MM/dd")
            : "";
    }

    private static string FormatDateTime(DateTime dateTime)
    {
        return dateTime.ToString("yyyy/MM/dd HH:mm");
    }

    private static string FormatPrice(decimal? price)
    {
        return price.HasValue
            ? price.Value.ToString("0")
            : "";
    }

    public IActionResult Index(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        string? sortOrder,
        int page = 1)
    {
        const int pageSize = 20;

        if (!string.IsNullOrWhiteSpace(keyword) && keyword.Length > 100)
        {
            ModelState.AddModelError(nameof(keyword), "検索キーワードは100文字以内で入力してください。");
            keyword = keyword[..100];
        }

        if (!string.IsNullOrWhiteSpace(category) && !ValidCategories.Contains(category))
        {
            ModelState.AddModelError(nameof(category), "不正なカテゴリが指定されています。");
            category = null;
        }

        if (!string.IsNullOrWhiteSpace(location) && !ValidLocations.Contains(location))
        {
            ModelState.AddModelError(nameof(location), "不正な保管場所が指定されています。");
            location = null;
        }

        if (!string.IsNullOrWhiteSpace(status) && !ValidStatuses.Contains(status))
        {
            ModelState.AddModelError(nameof(status), "不正な状態が指定されています。");
            status = null;
        }

        if (string.IsNullOrWhiteSpace(sortOrder))
        {
            sortOrder = "managementNumber";
        }

        if (!SortOptions.ContainsKey(sortOrder))
        {
            ModelState.AddModelError(nameof(sortOrder), "不正な並び順が指定されています。");
            sortOrder = "managementNumber";
        }

        if (string.IsNullOrWhiteSpace(confirmationFilter))
        {
            confirmationFilter = "all";
        }

        if (!ConfirmationFilterOptions.ContainsKey(confirmationFilter))
        {
            confirmationFilter = "all";
        }

        if (page < 1)
        {
            page = 1;
        }

        var today = DateTime.Today;

        var totalCount = _itemRepository.CountSearch(
            keyword,
            category,
            location,
            status,
            confirmationFilter,
            today);

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var warrantyAlertLimitDate = today.AddDays(30);

        var expiredWarrantyCount = _itemRepository.CountExpiredWarranty(
            keyword,
            category,
            location,
            status,
            confirmationFilter,
            today);

        var expiringWarrantyCount = _itemRepository.CountExpiringWarranty(
            keyword,
            category,
            location,
            status,
            confirmationFilter,
            today,
            warrantyAlertLimitDate);

        var statusCounts = _itemRepository.CountByStatus(
            keyword,
            category,
            location,
            status,
            confirmationFilter,
            today);

        var categoryCounts = _itemRepository.CountByCategory(
            keyword,
            category,
            location,
            status,
            confirmationFilter,
            today);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var items = _itemRepository.Search(
          keyword,
          category,
          location,
          status,
          confirmationFilter,
          sortOrder,
          page,
          pageSize,
          today);

        var viewModel = new ItemIndexViewModel
        {
            Items = items,
            Keyword = keyword,
            Category = category,
            Location = location,
            Status = status,
            ConfirmationFilter = confirmationFilter,
            SortOrder = sortOrder,
            CurrentPage = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            ExpiredWarrantyCount = expiredWarrantyCount,
            ExpiringWarrantyCount = expiringWarrantyCount,
            StatusCounts = statusCounts,
            CategoryCounts = categoryCounts
        };

        SetSelectLists();

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult ExportCsv(
    string? keyword,
    string? category,
    string? location,
    string? status,
    string? confirmationFilter,
    string? sortOrder)
    {
        if (!string.IsNullOrWhiteSpace(keyword) && keyword.Length > 100)
        {
            keyword = keyword[..100];
        }

        if (!string.IsNullOrWhiteSpace(category) && !ValidCategories.Contains(category))
        {
            category = null;
        }

        if (!string.IsNullOrWhiteSpace(location) && !ValidLocations.Contains(location))
        {
            location = null;
        }

        if (!string.IsNullOrWhiteSpace(status) && !ValidStatuses.Contains(status))
        {
            status = null;
        }

        if (string.IsNullOrWhiteSpace(sortOrder))
        {
            sortOrder = "managementNumber";
        }

        if (!SortOptions.ContainsKey(sortOrder))
        {
            sortOrder = "managementNumber";
        }

        if (string.IsNullOrWhiteSpace(confirmationFilter))
        {
            confirmationFilter = "all";
        }

        if (!ConfirmationFilterOptions.ContainsKey(confirmationFilter))
        {
            confirmationFilter = "all";
        }

        var today = DateTime.Today;

        var items = _itemRepository.SearchForCsv(
            keyword,
            category,
            location,
            status,
            confirmationFilter,
            sortOrder,
            today);

        var csv = BuildCsv(items);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        var preamble = encoding.GetPreamble();
        var csvBytes = encoding.GetBytes(csv);

        var bytes = preamble.Concat(csvBytes).ToArray();

        var fileName = $"備品一覧_{DateTime.Now:yyyyMMddHHmmss}.csv";

        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    [HttpGet]
    public IActionResult Create()
    {
        SetSelectLists();

        return View(new CreateItemViewModel
        {
            PurchaseDate = DateTime.Today,
            Status = "使用中"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(CreateItemViewModel viewModel)
    {
        if (viewModel.PurchaseDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(viewModel.PurchaseDate), "購入日は未来の日付にできません。");
        }

        ValidatePurchasePrice(viewModel.PurchasePrice, nameof(viewModel.PurchasePrice));
        ValidateWarrantyUntil(viewModel.PurchaseDate, viewModel.WarrantyUntil, nameof(viewModel.WarrantyUntil));

        if (!ValidStatuses.Contains(viewModel.Status))
        {
            ModelState.AddModelError(nameof(viewModel.Status), "不正な状態が指定されています。");
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Category) &&
            !ValidCategories.Contains(viewModel.Category))
        {
            ModelState.AddModelError(nameof(viewModel.Category), "不正なカテゴリが指定されています。");
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Location) &&
            !ValidLocations.Contains(viewModel.Location))
        {
            ModelState.AddModelError(nameof(viewModel.Location), "不正な保管場所が指定されています。");
        }

        if (!ModelState.IsValid)
        {
            SetSelectLists();
            return View(viewModel);
        }

        var item = new Item
        {
            Name = viewModel.Name,
            PurchaseDate = viewModel.PurchaseDate,
            PurchasePrice = viewModel.PurchasePrice,
            WarrantyUntil = viewModel.WarrantyUntil,
            Category = viewModel.Category,
            Location = viewModel.Location,
            AssignedUser = viewModel.AssignedUser,
            Status = viewModel.Status,
            Note = viewModel.Note,
            LastConfirmedAt = viewModel.LastConfirmedAt,
            ConfirmationNote = viewModel.ConfirmationNote
        };

        _itemRepository.Add(item);
        TempData["SuccessMessage"] = "備品を登録しました。";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var item = _itemRepository.GetById(id);

        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var item = _itemRepository.GetById(id);

        if (item is null)
        {
            return NotFound();
        }

        var viewModel = new EditItemViewModel
        {
            Id = item.Id,
            ManagementNumber = item.ManagementNumber,
            Name = item.Name,
            PurchaseDate = item.PurchaseDate,
            PurchasePrice = item.PurchasePrice,
            WarrantyUntil = item.WarrantyUntil,
            Category = item.Category,
            Location = item.Location,
            AssignedUser = item.AssignedUser,
            Status = item.Status,
            Note = item.Note,
            LastConfirmedAt = item.LastConfirmedAt,
            ConfirmationNote = item.ConfirmationNote
        };

        SetSelectLists();

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditItemViewModel viewModel)
    {
        if (viewModel.Id <= 0)
        {
            return BadRequest();
        }

        var existingItem = _itemRepository.GetById(viewModel.Id);

        if (existingItem is null)
        {
            return NotFound();
        }

        if (viewModel.PurchaseDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(viewModel.PurchaseDate), "購入日は未来の日付にできません。");
        }

        ValidatePurchasePrice(viewModel.PurchasePrice, nameof(viewModel.PurchasePrice));
        ValidateWarrantyUntil(viewModel.PurchaseDate, viewModel.WarrantyUntil, nameof(viewModel.WarrantyUntil));

        if (!ValidStatuses.Contains(viewModel.Status))
        {
            ModelState.AddModelError(nameof(viewModel.Status), "不正な状態が指定されています。");
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Category) &&
            !ValidCategories.Contains(viewModel.Category))
        {
            ModelState.AddModelError(nameof(viewModel.Category), "不正なカテゴリが指定されています。");
        }

        if (!string.IsNullOrWhiteSpace(viewModel.Location) &&
            !ValidLocations.Contains(viewModel.Location))
        {
            ModelState.AddModelError(nameof(viewModel.Location), "不正な保管場所が指定されています。");
        }

        if (!ModelState.IsValid)
        {
            viewModel.ManagementNumber = existingItem.ManagementNumber;
            SetSelectLists();
            return View(viewModel);
        }

        var item = new Item
        {
            Id = viewModel.Id,
            Name = viewModel.Name,
            PurchaseDate = viewModel.PurchaseDate,
            PurchasePrice = viewModel.PurchasePrice,
            WarrantyUntil = viewModel.WarrantyUntil,
            Category = viewModel.Category,
            Location = viewModel.Location,
            AssignedUser = viewModel.AssignedUser,
            Status = viewModel.Status,
            Note = viewModel.Note,
            LastConfirmedAt = viewModel.LastConfirmedAt,
            ConfirmationNote = viewModel.ConfirmationNote
        };

        _itemRepository.Update(item);
        TempData["SuccessMessage"] = "備品情報を更新しました。";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAsConfirmed(int id, string? returnUrl)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var updated = _itemRepository.MarkAsConfirmed(id);

        if (!updated)
        {
            TempData["ErrorMessage"] = "対象の備品が見つかりませんでした。";
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "確認日を今日に更新しました。";

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkAsDisposed(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var updated = _itemRepository.MarkAsDisposed(id);

        if (!updated)
        {
            TempData["ErrorMessage"] = "対象の備品が見つかりませんでした。";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "備品の状態を廃棄済みに変更しました。";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        if (id <= 0)
        {
            return BadRequest();
        }

        var deleted = _itemRepository.Delete(id);

        if (!deleted)
        {
            TempData["ErrorMessage"] = "台帳から非表示にできるのは、状態が廃棄済みの備品だけです。";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "廃棄済みの備品を台帳から非表示にしました。";

        return RedirectToAction(nameof(Index));
    }
}
