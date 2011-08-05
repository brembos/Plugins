using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Phoenix.Sales.Plugins
{
    public interface IServiceContextFactory
    {
        OrganizationServiceContext GetOrganizationServiceContext(IOrganizationService service);
    }

    public class ServiceContextFactory : IServiceContextFactory
    {
        public virtual OrganizationServiceContext GetOrganizationServiceContext(IOrganizationService service)
        {
            return new OrganizationServiceContext(service);
        }
    }
}