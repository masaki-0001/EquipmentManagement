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

    private void SetSelectLists()
    {
        ViewBag.Statuses = ValidStatuses;
        ViewBag.Categories = ValidCategories;
        ViewBag.Locations = ValidLocations;
    }

    private void ValidatePurchasePrice(decimal? purchasePrice, string fieldName)
    {
        if (purchasePrice.HasValue &&
            decimal.Truncate(purchasePrice.Value) != purchasePrice.Value)
        {
            ModelState.AddModelError(fieldName, "購入金額は1円単位の整数で入力してください。");
        }
    }

    public IActionResult Index(
        string? keyword,
        string? category,
        string? location,
        string? status)
    {
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

        var items = _itemRepository.Search(keyword, category, location, status);

        ViewBag.Keyword = keyword;
        ViewBag.SelectedCategory = category;
        ViewBag.SelectedLocation = location;
        ViewBag.SelectedStatus = status;
        SetSelectLists();

        return View(items);
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
            Category = viewModel.Category,
            Location = viewModel.Location,
            AssignedUser = viewModel.AssignedUser,
            Status = viewModel.Status
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
            Category = item.Category,
            Location = item.Location,
            AssignedUser = item.AssignedUser,
            Status = item.Status
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
            Category = viewModel.Category,
            Location = viewModel.Location,
            AssignedUser = viewModel.AssignedUser,
            Status = viewModel.Status
        };

        _itemRepository.Update(item);
        TempData["SuccessMessage"] = "備品情報を更新しました。";

        return RedirectToAction(nameof(Index));
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