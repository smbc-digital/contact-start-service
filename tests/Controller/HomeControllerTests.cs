using contact_start_service.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StockportGovUK.AspNetCore.Availability.Managers;
using Xunit;

namespace contact_start_service_tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly HomeController _homeController;
        private readonly Mock<IAvailabilityManager> _mockAvailabilityManager = new Mock<IAvailabilityManager>();

        public HomeControllerTests()
        {
            _homeController = new HomeController(_mockAvailabilityManager.Object);
        }

        [Fact]
        public void Get_ShouldReturnOK()
        {
            // Act
            var response = _homeController.Get();
            var statusResponse = response as OkResult;
            
            // Assert
            Assert.NotNull(statusResponse);
            Assert.Equal(200, statusResponse.StatusCode);
        }

        [Fact]
        public void Post_ShouldReturnOK()
        {
            // Arrange
            _mockAvailabilityManager
                .Setup(_ => _.IsFeatureEnabled(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var response = _homeController.Post();
            var statusResponse = response as OkResult;
            
            // Assert
            Assert.NotNull(statusResponse);
            Assert.Equal(200, statusResponse.StatusCode);
        }
    }
}
