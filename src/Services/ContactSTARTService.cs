using contact_start_service.Config;
using contact_start_service.Extensions;
using contact_start_service.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways.MailingServiceGateway;
using StockportGovUK.NetStandard.Gateways.VerintServiceGateway;
using StockportGovUK.NetStandard.Models.Enums;
using StockportGovUK.NetStandard.Models.Mail;
using System;
using System.Threading.Tasks;

namespace contact_start_service.Services
{
    public class ContactSTARTService : IContactSTARTService
    {
        private readonly IVerintServiceGateway verintServiceGateway;
        private readonly VerintConfiguration verintConfiguration;
        private readonly IMailingServiceGateway mailingServiceGateway;
        public ContactSTARTService(
            IVerintServiceGateway _verintServiceGateway,
            IMailingServiceGateway _mailingServiceGateway,
            IOptions<VerintConfiguration> _verintConfiguration)
        {
            verintServiceGateway = _verintServiceGateway;
            mailingServiceGateway = _mailingServiceGateway;
            verintConfiguration = _verintConfiguration.Value;
        }

        public async Task<string> CreateCase(ContactSTARTRequest request)
        {
            if (!verintConfiguration.ClassificationMap.TryGetValue(request.AreaOfConcern.Trim(), out var eventCode))
                throw new Exception("ContactSTARTService.CreateCase: EventCode not found");

            var response = await verintServiceGateway.CreateCase(request.MapToCase(eventCode));

            if (!response.IsSuccessStatusCode)
                throw new Exception($"ContactSTARTService.CreateCase: the status code {response.StatusCode} indicates something has gone wrong when attempting to create a case within verint-service.");

            if (request.IsAboutSelf && !string.IsNullOrEmpty(request.RefereePerson.EmailAddress))
                _ = mailingServiceGateway.Send(new Mail
                {
                    Template = EMailTemplate.ContactStartRequest,
                    Payload = JsonConvert.SerializeObject(new
                    {
                        Reference = response.ResponseContent,
                        request.RefereePerson.FirstName,
                        RecipientAddress = request.RefereePerson.EmailAddress,
                        Subject = "Thank you for contacting START"
                    })
                });

            return response.ResponseContent;
        }
    }
}
