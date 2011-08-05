using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Moq;
using NUnit.Framework;

namespace Phoenix.Sales.Plugins.UnitTests
{
    [TestFixture]
    public class SamplePluginTests
    {
        private IServiceProvider serviceProvider;
        private Mock<IPluginExecutionContext> contextMock;

        [TestFixtureSetUp]
        public void SetupServiceProvider()
        {
            var serviceStub = new Mock<IServiceProvider>();
            serviceProvider = serviceStub.Object;
            contextMock = new Mock<IPluginExecutionContext>();
            contextMock.Setup(x => x.Depth).Returns(1);
            serviceStub.Setup(x => x.GetService(typeof(IPluginExecutionContext))).Returns(contextMock.Object);
            var pipelineContext = new Mock<IOrganizationServiceFactory>();
            serviceStub.Setup(x => x.GetService(typeof(IOrganizationServiceFactory))).Returns(pipelineContext.Object);
        }

        [Test]
        public void TestHelloWorld()
        {
            //Arrange
            var entity = new Entity("accountnumber");
            var item = new KeyValuePair<string, object>("Target", entity);
            var parameters = new ParameterCollection { item };
            contextMock.Setup(x => x.InputParameters).Returns(parameters);
            //Act
            var plugin = new SamplePlugin();
            plugin.Execute(serviceProvider);
            //Assert
            var actual = ((Entity)contextMock.Object.InputParameters["Target"])["accountnumber"];
            Assert.AreEqual("Hello World!", actual);
        }
    }
}