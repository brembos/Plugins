using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Phoenix.Sales.Plugins.UnitTests.Wrappers
{

    public class ServiceContextWrapper : OrganizationServiceContext
    {
        public ICollection<Entity> Data { get; private set; }

        public ServiceContextWrapper(IOrganizationService service, ICollection<Entity> data)
            : base(service)
        {
            Data = data;
        }

        protected override IQueryable<TEntity> CreateQuery<TEntity>(IQueryProvider provider, string entityLogicalName)
        {
            return Data.OfType<TEntity>().Where(data =>
            {
                var entityData = data as Entity;
                if (entityData != null)
                    return entityData.LogicalName == entityLogicalName;
                return false;
            }).AsQueryable();
        }
    }
}
