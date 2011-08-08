using System;
using System.Linq;
using Microsoft.Xrm.Sdk;

namespace Phoenix.Sales.Plugins
{
    public class ContractNumberPlugin : IPlugin
    {
        private static readonly object syncLock = new object();
        readonly IServiceContextFactory serviceContextFactory;

        public const string ContractNumberFieldName = "global_contractnumber";

        public ContractNumberPlugin()
        {
            serviceContextFactory = new ServiceContextFactory();
        }

        public ContractNumberPlugin(IServiceContextFactory factory)
        {
            serviceContextFactory = factory;
        }

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
            var orgContext = serviceContextFactory.GetOrganizationServiceContext(service);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            if (tracingService == null) throw new PluginException("Failed to retrieve the tracing service");
            tracingService.Trace("Entering Execute on ContractNumberPlugin");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                lock (syncLock)
                {
                    var entity = context.InputParameters["Target"] as Entity;
                    if (entity != null)
                    {
                        if (entity.LogicalName != "global_contract")
                            throw new PluginException("Entity is not of correct type. Check LogicalName.");


                        var contractNumbers = orgContext.CreateQuery("global_contract")
                            .Where(c => c["global_contractnumber"] != null)
                            .Select(c => c["global_contractnumber"].ToString()).ToList();
                        var numbers = contractNumbers.Where(IsNumeric).Select(int.Parse).ToList();
                        int count = numbers.Count();
                        tracingService.Trace("Found: ", count);

                        int current = 0;
                        if (count > 0)
                        {
                            current = numbers.Max();
                        }

                        current = current + 1;
                        entity[ContractNumberFieldName] = current.ToString();
                        service.Update(entity);
                    }
                }
            }
        }

        private bool IsNotNullAndIsNumeric(object attribute)
        {
            return attribute != null && IsNumeric(attribute.ToString());
        }

        private bool IsNumeric(string text)
        {
            int result;
            return int.TryParse(text, out result);
        }
    }
}