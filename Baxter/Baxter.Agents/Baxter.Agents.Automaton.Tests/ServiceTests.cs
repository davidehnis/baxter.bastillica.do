using System;
using System.Security.Cryptography.X509Certificates;
using Baxter.Agents.Automaton.Tests.AutomatonService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Baxter.Agents.Automaton.Tests
{
    [TestClass]
    public class ServiceTests
    {
        public void PostJobWithoutErrors()
        {
            // Arrange
            var service = new AutomatonClient();
            var observer = new TestObserver();

            // Act
            service.Post(observer, "4+2", null);

            // Assert
        }

        [TestMethod]
        public void SimpleCallToServiceWorksReturnsCorrectly()
        {
            // Arrange
            AutomatonClient service = new AutomatonClient();

            // Act
            var data = service.GetData(7);

            // Assert
            Assert.IsNotNull(data);
        }

        [TestMethod]
        public void UsingObserverToRetrieveData()
        {
            // Arrange
            var service = new AutomatonClient();
            var observer = new TestObserver();
            var value = 0;
            observer.Callback = (x) => { value = (int)x; };

            // Act
            service.Post(observer, "4+2", null);

            // Assert
            Assert.IsNotNull(value);
            Assert.IsTrue(value == 6);
        }
    }
}