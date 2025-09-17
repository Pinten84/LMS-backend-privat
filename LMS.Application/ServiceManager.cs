
using LMS.Application.Contracts;
namespace LMS.Application
{
	public class ServiceManager : IServiceManager
	{
		private readonly Lazy<IAuthService> authService;

		public IAuthService AuthService => authService.Value;

		public ServiceManager(Lazy<IAuthService> authService)
		{
			this.authService = authService;
		}
	}
}
