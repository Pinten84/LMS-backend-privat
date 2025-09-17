using LMS.Domain.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace LMS.Infrastructure.Repositories;
public class ModuleRepository
{
            private readonly ApplicationDbContext _context;
            public ModuleRepository(ApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<List<Module>> GetAllAsync()
            {
                return await _context.Modules.Include(m => m.Activities).ToListAsync();
            }
            public async Task<(List<Module> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search)
            {
                var query = _context.Modules
                    .Include(m => m.Activities)
                    .AsQueryable();
                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(m => m.Name.Contains(search) || m.Description.Contains(search));
                var total = await query.CountAsync();
                var items = await query.OrderBy(m => m.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                return (items, total);
            }
            public async Task<Module?> GetByIdAsync(int id)
            {
                return await _context.Modules.Include(m => m.Activities).FirstOrDefaultAsync(m => m.Id == id);
            }
            public async Task AddAsync(Module module)
            {
                var overlapping = await _context.Modules
                    .AnyAsync(m => m.CourseId == module.CourseId &&
                                  ((module.StartDate < m.EndDate && module.EndDate > m.StartDate) ||
                                   module.StartDate < m.StartDate && module.EndDate > m.EndDate));
                if (overlapping)
                    throw new InvalidOperationException("Modulen överlappar med en annan modul i kursen.");
                var course = await _context.Courses.FindAsync(module.CourseId);
                if (course == null)
                    throw new InvalidOperationException("Kursen finns inte.");
                if (module.StartDate < course.StartDate)
                    throw new InvalidOperationException("Modulen startar före kursen.");
                if (course.EndDate.HasValue && module.EndDate > course.EndDate.Value)
                    throw new InvalidOperationException("Modulen slutar efter kursens slutdatum.");
                if (module.EndDate < module.StartDate)
                    throw new InvalidOperationException("Modulens slutdatum kan inte vara före startdatum.");
                _context.Modules.Add(module);
                await _context.SaveChangesAsync();
            }
            public async Task UpdateAsync(Module module)
            {
                // Reload current persisted values for validation context
                var course = await _context.Courses.FindAsync(module.CourseId) ?? throw new InvalidOperationException("Kursen finns inte.");
                if (module.StartDate < course.StartDate)
                    throw new InvalidOperationException("Modulen startar före kursen.");
                if (course.EndDate.HasValue && module.EndDate > course.EndDate.Value)
                    throw new InvalidOperationException("Modulen slutar efter kursens slutdatum.");
                if (module.EndDate < module.StartDate)
                    throw new InvalidOperationException("Modulens slutdatum kan inte vara före startdatum.");
                var overlapping = await _context.Modules
                    .AnyAsync(m => m.CourseId == module.CourseId && m.Id != module.Id &&
                                   ((module.StartDate < m.EndDate && module.EndDate > m.StartDate) ||
                                    module.StartDate < m.StartDate && module.EndDate > m.EndDate));
                if (overlapping)
                    throw new InvalidOperationException("Modulen överlappar med en annan modul i kursen.");
                _context.Modules.Update(module);
                await _context.SaveChangesAsync();
            }
            public async Task DeleteAsync(int id)
            {
                var module = await _context.Modules.FindAsync(id);
                if (module != null)
                {
                    _context.Modules.Remove(module);
                    await _context.SaveChangesAsync();
                }
            }
}
