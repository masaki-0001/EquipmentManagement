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
        ItemRepository.Add(item);

        return RedirectToAction(nameof(Index));
    }
}