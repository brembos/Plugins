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
        ContractNumberPlugin plugin;
        IServiceProvider serviceProvider;
        Mock<IPluginExecutionContext> contextMock;
        ParameterCollection parameters;
        private IList<Entity> list;

        [SetUp]
        public void SetupServiceProvider()
        {
            parameters = new ParameterCollection();
            list = new List<Entity>();
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
            plugin = new ContractNumberPlugin(new FakeServiceContextFactory(list));
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
        public void GlobalContractNumberIgnoreLetters()
        {
            //Arrange
            list.Add(new Entity("global_contract") { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("global_contractnumber", "XX-123") } });
            list.Add(new Entity("global_contract") { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("global_contractnumber", "122") } });
            list.Add(new Entity("global_contract") { Attributes = new AttributeCollection() { new KeyValuePair<string, object>("global_contractnumber", "121") } });
            //Act
            plugin.Execute(serviceProvider);
            //Assert
            object actual = ((Entity)contextMock.Object.InputParameters["Target"])["global_contractnumber"];
            Assert.AreEqual("123", actual);
        }

        [Test]
        [ExpectedException(typeof(PluginException), ExpectedMessage = "Context.Mode must be of type MessageProcessingMode.Synchronous.")]
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
        [ExpectedException(typeof(PluginException), ExpectedMessage = "Context.Stage must be of type MessageProcessingStage.PostOperation.")]
        public void PluginThrowsExceptionIfNotPostOperation(int stage)
        {
            contextMock.Setup(x => x.Stage).Returns(stage);
            plugin.Execute(serviceProvider);
        }


        [Test]
        [ExpectedException(typeof(PluginException), ExpectedMessage = "Entity is not of correct type. Check LogicalName.")]
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