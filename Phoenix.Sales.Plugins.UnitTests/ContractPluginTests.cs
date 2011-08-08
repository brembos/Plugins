using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Moq;
using NUnit.Framework;
using Phoenix.Sales.Plugins.UnitTests.Wrappers;

namespace Phoenix.Sales.Plugins.UnitTests
{
    [TestFixture]
    public class ContractPluginTests
    {
        private IServiceProvider serviceProvider;
        private Mock<IPluginExecutionContext> contextMock;
        private readonly ParameterCollection parameters = new ParameterCollection();
        ContractNumberPlugin plugin;
        IList<Entity> list = new List<Entity>();

        [TestFixtureSetUp]
        public void SetupServiceProvider()
        {
            plugin = GetPlugin();
        }

        private ContractNumberPlugin GetPlugin()
        {
            contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(x => x.Depth).Returns(1);
            contextMock.Setup(x => x.Stage).Returns(MessageProcessingStage.PostOperation);
            contextMock.Setup(x => x.Mode).Returns(MessageProcessingMode.Synchronous);
            contextMock.Setup(x => x.InputParameters).Returns(parameters);
            var serviceStub = new Mock<IServiceProvider>();
            serviceProvider = serviceStub.Object;
            serviceStub.Setup(x => x.GetService(typeof(IPluginExecutionContext))).Returns(contextMock.Object);
            var factory = new Mock<IOrganizationServiceFactory>();
            serviceStub.Setup(x => x.GetService(typeof(IOrganizationServiceFactory))).Returns(factory.Object);
            serviceStub.Setup(x => x.GetService(typeof(ITracingService))).Returns(new Mock<ITracingService>().Object);
            var orgServiceMock = new Mock<IOrganizationService>();
            factory.Setup(x => x.CreateOrganizationService(It.IsAny<Guid>())).Returns(orgServiceMock.Object);

            var entity = new Entity("global_contractnumber") { LogicalName = "global_contract" };
            var item = new KeyValuePair<string, object>("Target", entity);
            parameters.Add(item);

            return new ContractNumberPlugin(new FakeServiceContextFactory(list));
        }


        [Test]
        public void GlobalContractNumberAddingOneToExisting()
        {
            //Arrange
            list.Add(new Entity("global_contract") { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("global_contractnumber", "123") } });
            list.Add(new Entity("global_contract") { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("global_contractnumber", "122") } });
            list.Add(new Entity("global_contract") { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("global_contractnumber", "121") } });
            //Act
            plugin.Execute(serviceProvider);
            //Assert
            object actual = ((Entity)contextMock.Object.InputParameters["Target"])["global_contractnumber"];
            Assert.AreEqual("124", actual);
        }

        [Test]
        [ExpectedException(typeof(PluginException))]
        public void PluginMustNotBeAsync()
        {
            contextMock.Setup(x => x.Mode).Returns(MessageProcessingMode.Asynchronous);
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
            plugin.Execute(serviceProvider);
        }


        [Test]
        [ExpectedException(typeof(PluginException))]
        public void WrongEntityType()
        {
            parameters.Clear();
            var entity = new Entity("global_contractnumber") { LogicalName = "global_contract_wrong_type" };
            var item = new KeyValuePair<string, object>("Target", entity);
            parameters.Add(item);
            plugin.Execute(serviceProvider);
        }
    }
}