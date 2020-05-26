using contact_start_service.Controllers;
using contact_start_service.Models;
using contact_start_service.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace contact_start_service_tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _homeController;
        private readonly Mock<IContactSTARTService> _mockContactSTARTService = new Mock<IContactSTARTService>();

        public HomeControllerTests()
        {
            _homeController = new HomeController(_mockContactSTARTService.Object);
        }

        [Fact]
        public async Task Post_ShouldReturnOK()
        {
            _mockContactSTARTService
                .Setup(_ => _.CreateCase(It.IsAny<ContactSTARTRequest>()))
                .ReturnsAsync("");

            var response = await _homeController.Post(It.IsAny<ContactSTARTRequest>());
            var statusResponse = Assert.IsAssignableFrom<OkObjectResult>(response);
            
            Assert.NotNull(statusResponse);
            Assert.Equal(200, statusResponse.StatusCode);

            _mockContactSTARTService
                .Verify(_ => _.CreateCase(It.IsAny<ContactSTARTRequest>()), Times.Once);
        }
    }
}
