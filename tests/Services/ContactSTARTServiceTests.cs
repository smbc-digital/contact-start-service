using contact_start_service.Config;
using contact_start_service.Models;
using contact_start_service.Services;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Verint;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Address = StockportGovUK.NetStandard.Models.Addresses.Address;

namespace contact_start_service_tests.Services
{
    public class ContactSTARTServiceTests
    {
        private readonly ContactSTARTService _service;
        private Mock<IVerintServiceGateway> _mockVerintService = new Mock<IVerintServiceGateway>();

        public ContactSTARTServiceTests()
        {
            var mockVerintConfiguration = new Mock<IOptions<VerintConfiguration>>();
            mockVerintConfiguration
                .SetupGet(_ => _.Value)
                .Returns(new VerintConfiguration
                {
                    ClassificationMap = new Dictionary<string, int>
                {
                    {"Alcohol", 2002927},
                    {"Drugs", 2002928},
                    {"General lifestyle advice", 2002922},
                    {"Healthy weight", 2002925},
                    {"Smoking", 2002924},
                    {"Healthy eating", 2002926},
                    {"More than one area", 2002921},
                    {"Other", 2002920},
                    {"Physical activity", 2002923}
                }
                });

            _service = new ContactSTARTService(_mockVerintService.Object, mockVerintConfiguration.Object);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintGateway()
        {
            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = true
                });

            await _service.CreateCase(basicRequest);

            _mockVerintService
                .Verify(_ => _.CreateCase(It.IsAny<Case>()), Times.Once);
        }

        [Theory]
        [InlineData("Alcohol", "Public Health > START > Alcohol", 2002927)]
        [InlineData("Drugs", "Public Health > START > Drugs", 2002928)]
        [InlineData("General lifestyle advice", "Public Health > START > General lifestyle advice", 2002922)]
        [InlineData("Physical activity", "Public Health > START > Physical activity", 2002923)]
        [InlineData("Healthy weight", "Public Health > START > Healthy weight", 2002925)]
        [InlineData("Other", "Public Health > START > Other", 2002920)]
        [InlineData("Smoking", "Public Health > START > Smoking", 2002924)]
        [InlineData("Healthy eating", "Public Health > START > Healthy eating", 2002926)]
        [InlineData("More than one area", "Public Health > START > More than one area", 2002921)]
        [InlineData("More than one area ", "Public Health > START > More than one area", 2002921)]
        [InlineData(" More than one area", "Public Health > START > More than one area", 2002921)]
        public async Task CreateCase_ShouldCallServiceWithCorrectClassification(string areaOfConcern, string classification, int classificationCode)
        {
            var request = basicRequest;
            request.AreaOfConcern = areaOfConcern;

            Case caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .Callback<Case>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = true
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.Equal(classification, caseRequest.Classification);
            Assert.Equal(classificationCode, caseRequest.EventCode);
        }


        [Fact]
        public async Task CreateCase_ShouldThrowExceptionWhenNoMatchingClassificationFound()
        {
            var request = new ContactSTARTRequest
            {
                AreaOfConcern = "NoMatchExample"
            };

            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .ReturnsAsync(new HttpResponse<string>());

            var result = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.CreateCase(request);
            });

            Assert.Equal("Classification EventCode not found", result.Message);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithCustomer()
        {
            // reporting self - no referer
            var request = new ContactSTARTRequest
            {
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address
                    {
                        AddressLine1 = "test line 1",
                        Postcode = "test postcode"
                    },
                    DateOfBirth = DateTime.MinValue,
                    EmailAddress = "test@test.com",
                    FirstName = "test first name",
                    LastName = "test last name",
                    PhoneNumber = "+44000000000",
                    TimeSlot = "8:30 - 12:30"
                }
            };

            Case caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .Callback<Case>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = true
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.Customer);
            Assert.NotNull(caseRequest.Description);

            // checking the description field contains the user who needs assistance from the START team
            Assert.Contains($"Name: {request.RefereePerson.FirstName} {request.RefereePerson.LastName}", caseRequest.Description);
            Assert.Contains($"Tel: {request.RefereePerson.PhoneNumber}", caseRequest.Description);
            Assert.Contains($"Call Time: {request.RefereePerson.TimeSlot}", caseRequest.Description);
            Assert.Contains($"Email: {request.RefereePerson.EmailAddress}", caseRequest.Description);
            Assert.Contains($"Date of Birth: {request.RefereePerson.DateOfBirth.ToShortDateString()}", caseRequest.Description);
            Assert.Contains($"Address: {request.RefereePerson.Address.AddressLine1}, {request.RefereePerson.Address.Postcode}", caseRequest.Description);

            // checking the user who needs assistent exists in the Customer object used for matching
            Assert.Equal(request.RefereePerson.FirstName, caseRequest.Customer.Forename);
            Assert.Equal(request.RefereePerson.LastName, caseRequest.Customer.Surname);
            Assert.Equal(request.RefereePerson.EmailAddress, caseRequest.Customer.Email);
            Assert.Equal(request.RefereePerson.PhoneNumber, caseRequest.Customer.Mobile);
            Assert.Equal(request.RefereePerson.DateOfBirth, caseRequest.Customer.DateOfBirth);
            Assert.Equal(request.RefereePerson.Address.AddressLine1, caseRequest.Customer.Address.AddressLine1);
            Assert.Equal(request.RefereePerson.Address.Postcode, caseRequest.Customer.Address.Postcode);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithAdditionalFields()
        {
            // reporting self - no referer
            var request = new ContactSTARTRequest
            {
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address()
                },
                MoreInfomation = "test more infomation"
            };

            Case caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .Callback<Case>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = true
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.Description);

            Assert.Contains($"Details: {request.MoreInfomation}", caseRequest.Description);
            Assert.Contains($"Primary concern: {request.AreaOfConcern}", caseRequest.Description);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithRefererDetails()
        {
            // reporting other person - has referer
            var request = new ContactSTARTRequest
            {
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address()
                },
                RefererPerson = new RefererPerson
                {
                    ConnectionAbout = "test connection about",
                    FirstName = "test referer first name",
                    LastName = "test referer last name",
                    Permissions = "test permissions",
                    PhoneNumber = "+44 0000000000"
                },
                MoreInfomation = "test more infomation"
            };

            Case caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .Callback<Case>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = true
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.Description);

            Assert.Contains($"(Lagan) Referer: {request.RefererPerson.FirstName} {request.RefererPerson.LastName}", caseRequest.Description);
            Assert.Contains($"Connection to the Referee: {request.RefererPerson.ConnectionAbout}", caseRequest.Description);
            Assert.Contains($"Contact number: {request.RefererPerson.PhoneNumber}", caseRequest.Description);
        }

        [Fact]
        public async Task CreateCase_ShouldReturnString()
        {
            var expectedResponse = "test";

            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = expectedResponse
                });

            var result = await _service.CreateCase(basicRequest);

            Assert.Equal(expectedResponse, result);
        }

        [Fact]
        public async Task CreateCase_ShouldThrowVerintStatusCodeException()
        {
            _mockVerintService
                .Setup(_ => _.CreateCase(It.IsAny<Case>()))
                .ReturnsAsync(new HttpResponse<string>
                {
                    IsSuccessStatusCode = false,
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await Assert.ThrowsAsync<Exception>(async () => await _service.CreateCase(basicRequest));

            Assert.Equal($"ContactSTARTService.CreateCase: the status code {HttpStatusCode.BadRequest} indicates something has gone wrong when attempting to create a case within verint-service.", result.Message);
        }

        private readonly ContactSTARTRequest basicRequest = new ContactSTARTRequest
        {
            AreaOfConcern = "Alcohol",
            RefereePerson = new RefereePerson
            {
                Address = new Address()
            }
        };
    }
}
