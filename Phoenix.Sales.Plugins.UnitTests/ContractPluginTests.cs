using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Moq;
using NUnit.Framework;

namespace Phoenix.Sales.Plugins.UnitTests
{
    [TestFixture]
    public class ContractPluginTests
    {
        private IServiceProvider serviceProvider;
        private Mock<IPluginExecutionContext> contextMock;
        private readonly ParameterCollection parameters = new ParameterCollection();

        [TestFixtureSetUp]
        public void SetupServiceProvider()
        {
            var serviceStub = new Mock<IServiceProvider>();
            serviceProvider = serviceStub.Object;
            contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(x => x.Depth).Returns(1);
            contextMock.Setup(x => x.Stage).Returns(MessageProcessingStage.PostOperation);
            contextMock.Setup(x => x.Mode).Returns(MessageProcessingMode.Synchronous);
            contextMock.Setup(x => x.InputParameters).Returns(parameters);
            serviceStub.Setup(x => x.GetService(typeof(IPluginExecutionContext))).Returns(contextMock.Object);
            var pipelineContext = new Mock<IOrganizationServiceFactory>();
            serviceStub.Setup(x => x.GetService(typeof(IOrganizationServiceFactory))).Returns(pipelineContext.Object);
        }

        [Test]
        public void GlobalContractNumberAttributeMustExist()
        {
            //Arrange
            var entity = new Entity("globalcontractnumber");
            var item = new KeyValuePair<string, object>("Target", entity);
            parameters.Add(item);
            //Act
            var plugin = new ContractNumberPlugin();
            plugin.Execute(serviceProvider);
            //Assert
            object actual = ((Entity)contextMock.Object.InputParameters["Target"])["globalcontractnumber"];
            Assert.AreEqual("12345", actual);
        }

        [Test]
        [ExpectedException(typeof(PluginException))]
        public void PluginMustNotBeAsync()
        {
            contextMock.Setup(x => x.Mode).Returns(MessageProcessingMode.Asynchronous);
            var plugin = new ContractNumberPlugin();
            plugin.Execute(serviceProvider);
        }

        [Test]
        [TestCase(MessageProcessingStage.PreOperation)]
        [TestCase(MessageProcessingStage.MainOperation)]
        [TestCase(MessageProcessingStage.PostOperationCRM4)]
        [TestCase(MessageProcessingStage.PreValidation)]
        [ExpectedException(typeof(PluginException))]
        public void PluginThrowsExceptionIfNotPostOperation(int stage)
        {
            contextMock.Setup(x => x.Stage).Returns(stage);
            var plugin = new ContractNumberPlugin();
            plugin.Execute(serviceProvider);
        }
    }
}