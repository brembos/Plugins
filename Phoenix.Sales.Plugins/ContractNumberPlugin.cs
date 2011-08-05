using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Phoenix.Sales.Plugins
{
    public class ContractNumberPlugin : IPlugin
    {
        private static readonly object syncLock = new object();

        public const string ContractNumberFieldName = "global_contractnumber";

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.Depth != 1) { return; }
            if (context.Stage != MessageProcessingStage.PostOperation)
                throw new PluginException("Context.Stage must be of type MessageProcessingStage.PostOperation.");
            if (context.Mode != MessageProcessingMode.Synchronous)
                throw new PluginException("Context.Mode must be of type MessageProcessingMode.Synchronous.");

            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            var orgContext = GetOrganizationServiceContext(service);

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                lock (syncLock)
                {
                    //var current = orgContext.CreateQuery("global_contract").Select(c => c.Attributes["global_contractnumber"]).Max().ToString();
                    var query = orgContext.CreateQuery("global_contract");
                    var select = query.Select(c => c.Attributes["global_contractnumber"].ToString());
                    var current = select.Max().ToString();
                    var entity = context.InputParameters["Target"] as Entity;
                    if (entity != null)
                    {
                        int maxValue;
                        if (int.TryParse(current, out maxValue))
                        {
                            maxValue = maxValue + 1;
                            entity[ContractNumberFieldName] = maxValue;
                        }
                    }
                }
            }
        }

        public virtual OrganizationServiceContext GetOrganizationServiceContext(IOrganizationService service)
        {
            return new OrganizationServiceContext(service);
        }
    }
}