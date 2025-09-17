using LMS.Domain.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS.Infrastructure.Repositories;

public class ActivityRepository
{
    private readonly ApplicationDbContext _context;
    public ActivityRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Activity>> GetAllAsync()
    {
        return await _context.Activities.Include(a => a.Documents).ToListAsync();
    }

    public async Task<(List<Activity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = _context.Activities
            .Include(a => a.Documents)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(a => a.Name.Contains(search) || a.Description.Contains(search));
        var total = await query.CountAsync();
        var items = await query.OrderBy(a => a.StartTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Activity?> GetByIdAsync(int id)
    {
        return await _context.Activities.Include(a => a.Documents).FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task AddAsync(Activity activity)
    {
        var overlapping = await _context.Activities
            .AnyAsync(a => a.ModuleId == activity.ModuleId &&
                          ((activity.StartTime < a.EndTime && activity.EndTime > a.StartTime) ||
                           activity.StartTime < a.StartTime && activity.EndTime > a.EndTime));
        if (overlapping)
            throw new InvalidOperationException("Aktiviteten överlappar med en annan aktivitet i modulen.");

        var module = await _context.Modules.FindAsync(activity.ModuleId);
        if (module == null)
            throw new InvalidOperationException("Modulen finns inte.");
        if (activity.StartTime < module.StartDate || activity.EndTime > module.EndDate)
            throw new InvalidOperationException("Aktiviteten ligger utanför modulens datum.");
        if (activity.EndTime < activity.StartTime)
            throw new InvalidOperationException("Aktivitetens sluttid kan inte vara före starttid.");

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Activity activity)
    {
        var module = await _context.Modules.FindAsync(activity.ModuleId) ?? throw new InvalidOperationException("Modulen finns inte.");
        if (activity.StartTime < module.StartDate || activity.EndTime > module.EndDate)
            throw new InvalidOperationException("Aktiviteten ligger utanför modulens datum.");
        if (activity.EndTime < activity.StartTime)
            throw new InvalidOperationException("Aktivitetens sluttid kan inte vara före starttid.");
        var overlapping = await _context.Activities
            .AnyAsync(a => a.ModuleId == activity.ModuleId && a.Id != activity.Id &&
                           ((activity.StartTime < a.EndTime && activity.EndTime > a.StartTime) ||
                            activity.StartTime < a.StartTime && activity.EndTime > a.EndTime));
        if (overlapping)
            throw new InvalidOperationException("Aktiviteten överlappar med en annan aktivitet i modulen.");
        _context.Activities.Update(activity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity != null)
        {
            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();
        }
    }
}
