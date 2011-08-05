using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Phoenix.Sales.Plugins.UnitTests
{
    public class OrganizationServiceContextWrapper : OrganizationServiceContext
    {
        private readonly IQueryable<Entity> queryable;
        private readonly IQueryProvider queryProvider;

        public OrganizationServiceContextWrapper(IOrganizationService service)
            : base(service)
        {
        }

        public OrganizationServiceContextWrapper(IOrganizationService service, IQueryProvider queryProvider)
            : base(service)
        {
            this.queryProvider = queryProvider;
        }

        public OrganizationServiceContextWrapper(IOrganizationService service, IQueryable<Entity> queryable)
            : base(service)
        {
            this.queryable = queryable;
        }


        protected override IQueryable<TEntity> CreateQuery<TEntity>(IQueryProvider provider, string entityLogicalName)
        {
            //override provider
            return base.CreateQuery<TEntity>(queryProvider, entityLogicalName);
        }

        public new IQueryable<Entity> CreateQuery(string entityLogicalName)
        {
            return queryable;
        }
    }
}
