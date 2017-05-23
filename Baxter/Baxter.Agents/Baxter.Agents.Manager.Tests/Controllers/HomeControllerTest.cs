using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Baxter.Agents.Manager;
using Baxter.Agents.Manager.Controllers;

namespace Baxter.Agents.Manager.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Home Page", result.ViewBag.Title);
        }
    }
}
