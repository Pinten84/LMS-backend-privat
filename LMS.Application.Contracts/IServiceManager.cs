namespace LMS.Application.Contracts;
public interface IServiceManager
{
    IAuthService AuthService
    {
        get;
    }
}
