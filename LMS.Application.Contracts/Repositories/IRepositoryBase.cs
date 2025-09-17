namespace LMS.Application.Contracts.Repositories;

public interface IRepositoryBase<T>
{
    void Create(T entity);
    void Update(T entity);
    void Delete(T entity);
}
