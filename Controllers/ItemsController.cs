using EquipmentManagement.Models;
using EquipmentManagement.Repositories;
using Microsoft.AspNetCore.Mvc;
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

    public IActionResult Index(string? keyword)
    {
        if (!string.IsNullOrWhiteSpace(keyword) && keyword.Length > 100)
        {
            ModelState.AddModelError(nameof(keyword), "検索キーワードは100文字以内で入力してください。");
            keyword = keyword[..100];
        }

        var items = _itemRepository.Search(keyword);

        ViewBag.Keyword = keyword;

        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
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

        if (!ValidStatuses.Contains(viewModel.Status))
        {
            ModelState.AddModelError(nameof(viewModel.Status), "不正な状態が指定されています。");
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var item = new Item
        {
            Name = viewModel.Name,
            PurchaseDate = viewModel.PurchaseDate,
            Status = viewModel.Status
        };

        _itemRepository.Add(item);

        return RedirectToAction(nameof(Index));
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
            Status = item.Status
        };

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

        if (!ValidStatuses.Contains(viewModel.Status))
        {
            ModelState.AddModelError(nameof(viewModel.Status), "不正な状態が指定されています。");
        }

        if (!ModelState.IsValid)
        {
            viewModel.ManagementNumber = existingItem.ManagementNumber;
            return View(viewModel);
        }

        var item = new Item
        {
            Id = viewModel.Id,
            Name = viewModel.Name,
            PurchaseDate = viewModel.PurchaseDate,
            Status = viewModel.Status
        };

        _itemRepository.Update(item);

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

        _itemRepository.Delete(id);

        return RedirectToAction(nameof(Index));
    }
}
