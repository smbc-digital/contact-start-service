using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using contact_start_service.Config;
using contact_start_service.Models;
using contact_start_service.Services;
using Microsoft.Extensions.Options;
using Moq;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.MailingService;
using StockportGovUK.NetStandard.Gateways.Models.Mail;
using StockportGovUK.NetStandard.Gateways.Models.Verint;
using StockportGovUK.NetStandard.Gateways.Models.Verint.VerintOnlineForm;
using StockportGovUK.NetStandard.Gateways.Response;
using StockportGovUK.NetStandard.Gateways.VerintService;
using Xunit;

using Address = StockportGovUK.NetStandard.Gateways.Models.Addresses.Address;

namespace contact_start_service_tests.Services
{
    public class ContactSTARTServiceTests
    {
        private readonly ContactSTARTService _service;
        private readonly Mock<IVerintServiceGateway> _mockVerintService = new();
        private readonly Mock<IMailingServiceGateway> _mockMailingService = new();

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

            _service = new ContactSTARTService(_mockVerintService.Object, _mockMailingService.Object, mockVerintConfiguration.Object);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintGateway()
        {
            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(basicRequest);

            _mockVerintService
                .Verify(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()), Times.Once);
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

            VerintOnlineFormRequest caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .Callback<VerintOnlineFormRequest>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.Equal(classification, caseRequest.VerintCase.Classification);
            Assert.Equal(classificationCode, caseRequest.VerintCase.EventCode);
        }


        [Fact]
        public async Task CreateCase_ShouldThrowExceptionWhenNoMatchingClassificationFound()
        {
            var request = new ContactSTARTRequest
            {
                AreaOfConcern = "NoMatchExample"
            };

            var result = await Assert.ThrowsAsync<Exception>(async () =>
            {
                await _service.CreateCase(request);
            });

            Assert.Equal("ContactSTARTService.CreateCase: EventCode not found", result.Message);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithCustomer()
        {
            // reporting self - no referer
            var request = new ContactSTARTRequest
            {
                AboutYourSelfRadio = "no",
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

            VerintOnlineFormRequest caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .Callback<VerintOnlineFormRequest>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.VerintCase.Customer);
            Assert.NotNull(caseRequest.VerintCase.Description);

            // checking the description field contains the user who needs assistance from the START team
            Assert.Contains($"Name: {request.RefereePerson.FirstName} {request.RefereePerson.LastName}", caseRequest.VerintCase.Description);
            Assert.Contains($"Tel: {request.RefereePerson.PhoneNumber}", caseRequest.VerintCase.Description);
            Assert.Contains($"Call Time: {request.RefereePerson.TimeSlot}", caseRequest.VerintCase.Description);
            Assert.Contains($"Email: {request.RefereePerson.EmailAddress}", caseRequest.VerintCase.Description);
            Assert.Contains($"Date of Birth: {request.RefereePerson.DateOfBirth.ToShortDateString()}", caseRequest.VerintCase.Description);
            Assert.Contains($"Address: {request.RefereePerson.Address.AddressLine1}, {request.RefereePerson.Address.Postcode}", caseRequest.VerintCase.Description);

            // checking the user who needs assistent exists in the Customer object used for matching
            Assert.Equal(request.RefereePerson.FirstName, caseRequest.VerintCase.Customer.Forename);
            Assert.Equal(request.RefereePerson.LastName, caseRequest.VerintCase.Customer.Surname);
            Assert.Equal(request.RefereePerson.EmailAddress, caseRequest.VerintCase.Customer.Email);
            Assert.Equal(request.RefereePerson.PhoneNumber, caseRequest.VerintCase.Customer.Telephone);
            Assert.Equal(request.RefereePerson.DateOfBirth, caseRequest.VerintCase.Customer.DateOfBirth);
            Assert.Equal(request.RefereePerson.Address.AddressLine1, caseRequest.VerintCase.Customer.Address.AddressLine1);
            Assert.Equal(request.RefereePerson.Address.Postcode, caseRequest.VerintCase.Customer.Address.Postcode);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithAdditionalFields()
        {
            // reporting self - no referer
            var request = new ContactSTARTRequest
            {
                AboutYourSelfRadio = "no",
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address()
                },
                MoreInfomation = "test more infomation"
            };

            VerintOnlineFormRequest caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .Callback<VerintOnlineFormRequest>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.VerintCase.Description);

            Assert.Contains($"Details: {request.MoreInfomation}", caseRequest.VerintCase.Description);
            Assert.Contains($"Primary concern: {request.AreaOfConcern}", caseRequest.VerintCase.Description);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithRefererDetails()
        {
            // reporting other person - has referer
            var request = new ContactSTARTRequest
            {
                AreaOfConcern = "Alcohol",
                AboutYourSelfRadio = "no",
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

            VerintOnlineFormRequest caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .Callback<VerintOnlineFormRequest>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.VerintCase.Description);

            Assert.Contains($"(Lagan) Referrer: {request.RefererPerson.FirstName} {request.RefererPerson.LastName}", caseRequest.VerintCase.Description);
            Assert.Contains($"Connection to the Referee: {request.RefererPerson.ConnectionAbout}", caseRequest.VerintCase.Description);
            Assert.Contains($"Contact number: {request.RefererPerson.PhoneNumber}", caseRequest.VerintCase.Description);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithoutTelNumberAndTimeSlot()
        {
            // reporting other person - has referer
            var request = new ContactSTARTRequest
            {
                AboutYourSelfRadio = "no",
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address()
                },
                MoreInfomation = "test more infomation"
            };

            VerintOnlineFormRequest caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .Callback<VerintOnlineFormRequest>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.VerintCase.Description);

            Assert.DoesNotContain($"Tel", caseRequest.VerintCase.Description);
            Assert.DoesNotContain($"Call Time", caseRequest.VerintCase.Description);
        }

        [Fact]
        public async Task CreateCase_ShouldCallVerintServiceWithTelNumberAndTimeSlot()
        {
            // reporting other person - has referer
            var request = new ContactSTARTRequest
            {
                AboutYourSelfRadio = "no",
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address(),
                    PhoneNumber = "+440000000000",
                    TimeSlot = "10:00 - 17:00"
                },
                MoreInfomation = "test more information"
            };

            VerintOnlineFormRequest caseRequest = null;

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .Callback<VerintOnlineFormRequest>(_ => caseRequest = _)
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            Assert.NotNull(caseRequest);
            Assert.NotNull(caseRequest.VerintCase.Description);

            Assert.Contains($"Tel: {request.RefereePerson.PhoneNumber}", caseRequest.VerintCase.Description);
            Assert.Contains($"Call Time: {request.RefereePerson.TimeSlot}", caseRequest.VerintCase.Description);
        }

        [Fact]
        public async Task CreateCase_ShouldReturnString()
        {
            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            var result = await _service.CreateCase(basicRequest);

            Assert.Equal("123456", result);
        }

        [Fact]
        public async Task CreateCase_ShouldThrowVerintStatusCodeException()
        {
            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = false,
                    StatusCode = HttpStatusCode.BadRequest
                });

            var result = await Assert.ThrowsAsync<Exception>(async () => await _service.CreateCase(basicRequest));

            Assert.Equal($"ContactSTARTService.CreateCase : the status code {HttpStatusCode.BadRequest} indicates something has gone wrong when attempting to create a case within verint-service.", result.Message);
        }

        [Fact]
        public async Task CreateCase_ShouldCallMailingGatewayWithCorrectPayload()
        {
            var request = new ContactSTARTRequest
            {
                AboutYourSelfRadio = "yes",
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address(),
                    PhoneNumber = "+440000000000",
                    TimeSlot = "10:00 - 17:00",
                    EmailAddress = "test@test.com"
                },
                MoreInfomation = "test more infomation"
            };

            var caseRef = "000000000";

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = caseRef
                    }
                });

            Mail mailModel = null;

            _mockMailingService
                .Setup(_ => _.Send(It.IsAny<Mail>()))
                .Callback<Mail>(_ => mailModel = _);

            await _service.CreateCase(request);

            _mockMailingService.Verify(_ => _.Send(It.IsAny<Mail>()), Times.Once);

            Assert.Contains(request.RefereePerson.EmailAddress, mailModel.Payload);
            Assert.Equal(EMailTemplate.ContactStartRequest, mailModel.Template);
            Assert.Contains(caseRef, mailModel.Payload);
        }


        [Fact]
        public async Task CreateCase_ShouldNotCallMailingGateway()
        {
            var request = new ContactSTARTRequest
            {
                AboutYourSelfRadio = "no",
                AreaOfConcern = "Alcohol",
                RefereePerson = new RefereePerson
                {
                    Address = new Address(),
                    PhoneNumber = "+440000000000",
                    TimeSlot = "10:00 - 17:00",
                    EmailAddress = "test@test.com"
                },
                MoreInfomation = "test more infomation"
            };

            _mockVerintService
                .Setup(_ => _.CreateVerintOnlineFormCase(It.IsAny<VerintOnlineFormRequest>()))
                .ReturnsAsync(new HttpResponse<VerintOnlineFormResponse>
                {
                    IsSuccessStatusCode = true,
                    ResponseContent = new VerintOnlineFormResponse
                    {
                        VerintCaseReference = "123456"
                    }
                });

            await _service.CreateCase(request);

            _mockMailingService.VerifyNoOtherCalls();
        }

        private readonly ContactSTARTRequest basicRequest = new ContactSTARTRequest
        {
            AboutYourSelfRadio = "no",
            AreaOfConcern = "Alcohol",
            RefereePerson = new RefereePerson
            {
                Address = new Address()
            }
        };
    }
}
