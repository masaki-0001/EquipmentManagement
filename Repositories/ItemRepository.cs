using EquipmentManagement.Data;
using EquipmentManagement.Models;

namespace EquipmentManagement.Repositories;

public class ItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public List<Item> Search(
        string? keyword,
        string? category,
        string? location,
        string? status)
    {
        var query = _context.Items
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.ManagementNumber.Contains(keyword) ||
                (x.AssignedUser != null && x.AssignedUser.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(location))
        {
            query = query.Where(x => x.Location == location);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return query
            .OrderBy(x => x.Id)
            .ToList();
    }

    public Item? GetById(int id)
    {
        return _context.Items
            .FirstOrDefault(x => x.Id == id && !x.IsDeleted);
    }

    public void Add(Item item)
    {
        using var transaction = _context.Database.BeginTransaction();

        try
        {
            var now = DateTime.Now;

            item.IsDeleted = false;
            item.CreatedAt = now;
            item.UpdatedAt = now;

            _context.Items.Add(item);
            _context.SaveChanges();

            item.ManagementNumber = $"ITEM-{item.Id:0000}";

            _context.SaveChanges();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    public void Update(Item item)
    {
        var existingItem = GetById(item.Id);

        if (existingItem is null)
        {
            return;
        }

        existingItem.Name = item.Name;
        existingItem.PurchaseDate = item.PurchaseDate;
        existingItem.PurchasePrice = item.PurchasePrice;
        existingItem.Status = item.Status;
        existingItem.Category = item.Category;
        existingItem.Location = item.Location;
        existingItem.AssignedUser = item.AssignedUser;
        existingItem.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var item = GetById(id);

        if (item is null)
        {
            return;
        }

        item.IsDeleted = true;
        item.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
    }
}