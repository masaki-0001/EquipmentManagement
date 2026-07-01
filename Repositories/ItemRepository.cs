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
    public int CountSearch(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        DateTime today)
    {
        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);

        return query.Count();
    }

    public int CountExpiredWarranty(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        DateTime today)
    {
        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);

        return query
            .Where(x => x.Status != "廃棄済み")
            .Where(x => x.WarrantyUntil.HasValue && x.WarrantyUntil.Value < today)
            .Count();
    }

    public int CountExpiringWarranty(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        DateTime today,
        DateTime limitDate)
    {
        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);

        return query
            .Where(x => x.Status != "廃棄済み")
            .Where(x =>
                x.WarrantyUntil.HasValue &&
                x.WarrantyUntil.Value >= today &&
                x.WarrantyUntil.Value <= limitDate)
            .Count();
    }

    private static IQueryable<Item> ApplyConfirmationFilter(
    IQueryable<Item> query,
    string? confirmationFilter,
    DateTime today)
    {
        return confirmationFilter switch
        {
            "unconfirmed" => query.Where(x => !x.LastConfirmedAt.HasValue),

            "over90days" => query.Where(x =>
                x.LastConfirmedAt.HasValue &&
                x.LastConfirmedAt.Value <= today.AddDays(-90)),

            "over180days" => query.Where(x =>
                x.LastConfirmedAt.HasValue &&
                x.LastConfirmedAt.Value <= today.AddDays(-180)),

            "over365days" => query.Where(x =>
                x.LastConfirmedAt.HasValue &&
                x.LastConfirmedAt.Value <= today.AddDays(-365)),

            _ => query
        };
    }

    public Dictionary<string, int> CountByStatus(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        DateTime today)
    {
        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);

        return query
            .GroupBy(x => x.Status)
            .Select(x => new
            {
                Status = x.Key,
                Count = x.Count()
            })
            .ToDictionary(x => x.Status, x => x.Count);
    }

    public Dictionary<string, int> CountByCategory(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        DateTime today)
    {
        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);

        return query
            .Select(x => x.Category)
            .AsEnumerable()
            .GroupBy(x => string.IsNullOrWhiteSpace(x) ? "未入力" : x)
            .ToDictionary(x => x.Key, x => x.Count());
    }

    public List<Item> Search(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        string? sortOrder,
        int page,
        int pageSize,
        DateTime today)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1)
        {
            pageSize = 20;
        }

        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);
        query = ApplySort(query, sortOrder);

        return query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public List<Item> SearchForCsv(
        string? keyword,
        string? category,
        string? location,
        string? status,
        string? confirmationFilter,
        string? sortOrder,
        DateTime today)
    {
        var query = BuildSearchQuery(keyword, category, location, status);
        query = ApplyConfirmationFilter(query, confirmationFilter, today);
        query = ApplySort(query, sortOrder);

        return query.ToList();
    }

    private IQueryable<Item> BuildSearchQuery(
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

        return query;
    }

    private static IQueryable<Item> ApplySort(IQueryable<Item> query, string? sortOrder)
    {
        return sortOrder switch
        {
            "purchaseDateDesc" => query
                .OrderByDescending(x => x.PurchaseDate)
                .ThenBy(x => x.ManagementNumber),

            "updatedAtDesc" => query
                .OrderByDescending(x => x.UpdatedAt)
                .ThenBy(x => x.ManagementNumber),

            "warrantyUntilAsc" => query
                .OrderBy(x => x.WarrantyUntil == null)
                .ThenBy(x => x.WarrantyUntil)
                .ThenBy(x => x.ManagementNumber),

            _ => query
                .OrderBy(x => x.ManagementNumber)
        };
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
        existingItem.WarrantyUntil = item.WarrantyUntil;
        existingItem.Status = item.Status;
        existingItem.Category = item.Category;
        existingItem.Location = item.Location;
        existingItem.AssignedUser = item.AssignedUser;
        existingItem.Note = item.Note;
        existingItem.LastConfirmedAt = item.LastConfirmedAt;
        existingItem.ConfirmationNote = item.ConfirmationNote;
        existingItem.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
    }

    public bool MarkAsConfirmed(int id)
    {
        var item = GetById(id);

        if (item is null)
        {
            return false;
        }

        item.LastConfirmedAt = DateTime.Today;
        item.UpdatedAt = DateTime.Now;

        _context.SaveChanges();

        return true;
    }

    public bool MarkAsDisposed(int id)
    {
        var item = GetById(id);

        if (item is null)
        {
            return false;
        }

        item.Status = "廃棄済み";
        item.AssignedUser = null;
        item.UpdatedAt = DateTime.Now;

        _context.SaveChanges();

        return true;
    }

    public bool Delete(int id)
    {
        var item = GetById(id);

        if (item is null)
        {
            return false;
        }

        if (item.Status != "廃棄済み")
        {
            return false;
        }

        item.IsDeleted = true;
        item.UpdatedAt = DateTime.Now;

        _context.SaveChanges();

        return true;
    }
}
