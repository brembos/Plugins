using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
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
        ContractNumberPlugin plugin;

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
            var orgServiceMock = new Mock<IOrganizationService>();

            var queryableStub = new Mock<IQueryable<Entity>>();
            var queryProviderStub = new Mock<IQueryProvider>();
            var orgContextStub = new Mock<OrganizationServiceContextWrapper>(orgServiceMock.Object, queryableStub.Object);
            //var orgContextStub = new Mock<OrganizationServiceContextWrapper>(orgServiceMock.Object, queryProviderStub.Object);


            var entity1 = new Entity();
            entity1.Attributes.Add("global_contractnumber", "00123");
            var list = new List<Entity>() { entity1 };

            queryProviderStub.Setup(x => x.CreateQuery<Entity>(It.IsAny<Expression>())).Returns(list.AsQueryable());

            plugin = new ContractNumberPluginMock(orgContextStub.Object);
        }




        [Test]
        public void GlobalContractNumberAttributeMustExist()
        {
            //Arrange
            var entity = new Entity(ContractNumberPlugin.ContractNumberFieldName);
            var item = new KeyValuePair<string, object>("Target", entity);
            parameters.Add(item);
            //Act
            plugin.Execute(serviceProvider);
            //Assert
            object actual = ((Entity)contextMock.Object.InputParameters["Target"])["global_contractnumber"];
            Assert.AreEqual("12345", actual);
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
    }

    public class ContractNumberPluginMock : ContractNumberPlugin
    {
        private readonly OrganizationServiceContext organizationServiceContext;

        public ContractNumberPluginMock(OrganizationServiceContext organizationServiceContext)
        {
            this.organizationServiceContext = organizationServiceContext;
        }

        public override OrganizationServiceContext GetOrganizationServiceContext(IOrganizationService service)
        {
            return organizationServiceContext;
        }
    }
}