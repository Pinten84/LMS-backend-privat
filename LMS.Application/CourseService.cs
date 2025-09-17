using LMS.Domain.Entities;
using LMS.Infrastructure.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application;

public class CourseService
{
    private readonly CourseRepository _courseRepository;
    public CourseService(CourseRepository courseRepository) => _courseRepository = courseRepository;

    public Task<List<Course>> GetAllCoursesAsync() => _courseRepository.GetAllAsync();
    public Task<Course?> GetCourseByIdAsync(int id) => _courseRepository.GetByIdAsync(id);
    public Task<(List<Course> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search) => _courseRepository.GetPagedAsync(page, pageSize, search);
    public Task AddCourseAsync(Course course) => _courseRepository.AddAsync(course);
    public Task UpdateCourseAsync(Course course) => _courseRepository.UpdateAsync(course);
    public Task DeleteCourseAsync(int id) => _courseRepository.DeleteAsync(id);
}
