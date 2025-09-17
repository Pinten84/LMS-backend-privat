using LMS.Domain.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS.Infrastructure.Repositories;

public class DocumentRepository
{
    private readonly ApplicationDbContext _context;
    public DocumentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Document>> GetAllAsync()
    {
        return await _context.Documents.Include(d => d.UploadedByUser).ToListAsync();
    }

    public async Task<(List<Document> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = _context.Documents
            .Include(d => d.UploadedByUser)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(d => d.Name.Contains(search) || d.Description.Contains(search));
        var total = await query.CountAsync();
        var items = await query.OrderByDescending(d => d.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        return await _context.Documents.Include(d => d.UploadedByUser).FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task AddAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document != null)
        {
            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
        }
    }
}
