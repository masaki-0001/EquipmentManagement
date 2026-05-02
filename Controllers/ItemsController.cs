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
}