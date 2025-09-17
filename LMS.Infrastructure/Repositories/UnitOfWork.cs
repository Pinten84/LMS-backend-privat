using LMS.Application.Contracts.Repositories;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;
public class UnitOfWork : IUnitOfWork
{
            private readonly ApplicationDbContext _context;
            public UnitOfWork(ApplicationDbContext context)
            {
                _context = context ?? throw new ArgumentNullException(nameof(context));
            }
            public async Task CompleteAsync() => await _context.SaveChangesAsync();
}
