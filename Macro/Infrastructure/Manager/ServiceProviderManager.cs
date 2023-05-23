using Kosher.DependencyInjection;

namespace Macro.Infrastructure.Manager
{
    public class ServiceDispatcher
    {
        private static ServiceContainer _serviceContainer;
        public static void SetContainer(ServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer;
        }
        public static T Resolve<T>()
        {
            return _serviceContainer.Resolve<T>();
        }
    }
}
