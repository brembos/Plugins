using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Phoenix.Sales.Plugins.UnitTests.Wrappers
{
    public class FakeServiceContextFactory : IServiceContextFactory
    {
        private readonly ICollection<Entity> data;

        public FakeServiceContextFactory(ICollection<Entity> data)
        {
            this.data = data;
        }

        public OrganizationServiceContext GetOrganizationServiceContext(IOrganizationService service)
        {
            return new ServiceContextWrapper(service, data);
        }
    }
}