using System;
using Microsoft.Xrm.Sdk;

namespace Phoenix.Sales.Plugins
{
    /// <summary>
    /// Please remember to sign this project with a strong name!
    /// 
    /// Message:    Create
    /// Stage:      Pre
    /// Mode:       Synchronous
    /// Entity:     account
    /// </summary>
    public class SamplePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var sdk = factory.CreateOrganizationService(context.UserId);
            var tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            if (context.Depth != 1) { return; }
            if (context.Stage == MessageProcessingStage.PreOperation) { }
            if (context.Mode == MessageProcessingMode.Synchronous) { }
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = context.InputParameters["Target"] as Entity;
                entity["accountnumber"] = "Hello World!";
            }
        }
    }
}
