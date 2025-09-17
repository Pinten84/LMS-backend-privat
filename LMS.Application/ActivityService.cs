using LMS.Domain.Entities;
using LMS.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application;

public class ActivityService
{
    private readonly ActivityRepository _activityRepository;
    public ActivityService(ActivityRepository activityRepository) => _activityRepository = activityRepository;

    public Task<List<Activity>> GetAllActivitiesAsync() => _activityRepository.GetAllAsync();
    public Task<Activity?> GetActivityByIdAsync(int id) => _activityRepository.GetByIdAsync(id);
    public Task<(List<Activity> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search) => _activityRepository.GetPagedAsync(page, pageSize, search);
    public Task AddActivityAsync(Activity activity) => _activityRepository.AddAsync(activity);
    public Task UpdateActivityAsync(Activity activity) => _activityRepository.UpdateAsync(activity);
    public Task DeleteActivityAsync(int id) => _activityRepository.DeleteAsync(id);
}
