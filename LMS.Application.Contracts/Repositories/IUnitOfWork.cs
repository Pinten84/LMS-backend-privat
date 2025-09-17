namespace LMS.Application.Contracts.Repositories;

public interface IUnitOfWork
{
    Task CompleteAsync();
}
