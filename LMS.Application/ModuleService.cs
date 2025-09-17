using LMS.Domain.Entities;
using LMS.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application;
public class ModuleService
{
            private readonly ModuleRepository _moduleRepository;
            public ModuleService(ModuleRepository moduleRepository) => _moduleRepository = moduleRepository;
            public Task<List<Module>> GetAllModulesAsync() => _moduleRepository.GetAllAsync();
            public Task<Module?> GetModuleByIdAsync(int id) => _moduleRepository.GetByIdAsync(id);
            public Task<(List<Module> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search) => _moduleRepository.GetPagedAsync(page, pageSize, search);
            public Task AddModuleAsync(Module module) => _moduleRepository.AddAsync(module);
            public Task UpdateModuleAsync(Module module) => _moduleRepository.UpdateAsync(module);
            public Task DeleteModuleAsync(int id) => _moduleRepository.DeleteAsync(id);
}
