
using System;
using Microsoft.Xrm.Sdk;

namespace Phoenix.Sales.Plugins
{
    public class ContractNumberPlugin : IPlugin
    {
        private static object syncLock = new object();


        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (context.Stage != MessageProcessingStage.PostOperation)
                throw new PluginException("Context.Stage must be of type MessageProcessingStage.PostOperation");
            if (context.Mode != MessageProcessingMode.Synchronous)
                throw new PluginException("Context.Mode must be of type MessageProcessingMode.Synchronous");

            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                lock (syncLock)
                {
                    var entity = context.InputParameters["Target"] as Entity;
                    entity["globalcontractnumber"] = "12345";
                }

            }
        }
    }
}
