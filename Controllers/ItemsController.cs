using EquipmentManagement.Models;
using EquipmentManagement.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentManagement.Controllers;

public class ItemsController : Controller
{
    public IActionResult Index()
    {
        var items = ItemRepository.GetAll();

        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new Item
        {
            PurchaseDate = DateTime.Today,
            Status = "使用中"
        });
    }

    [HttpPost]
    public IActionResult Create(Item item)
    {

        if (item.PurchaseDate > DateTime.Today)
        {
            ModelState.AddModelError(nameof(item.PurchaseDate), "購入日は未来の日付にできません。");
        }

        if (!ModelState.IsValid)
        {
            return View(item);
        }

        ItemRepository.Add(item);

        return RedirectToAction(nameof(Index));
    }
}