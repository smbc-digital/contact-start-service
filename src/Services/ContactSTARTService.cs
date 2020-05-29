using contact_start_service.Config;
using contact_start_service.Extensions;
using contact_start_service.Models;
using Microsoft.Extensions.Options;
using StockportGovUK.NetStandard.Gateways.VerintServiceGateway;
using System;
using System.Threading.Tasks;

namespace contact_start_service.Services
{
    public interface IContactSTARTService
    {
        Task<string> CreateCase(ContactSTARTRequest request);
    }

    public class ContactSTARTService : IContactSTARTService
    {
        private readonly IVerintServiceGateway verintServiceGateway;
        private readonly VerintConfiguration verintConfiguration;
        public ContactSTARTService(IVerintServiceGateway _verintServiceGateway, IOptions<VerintConfiguration> _verintConfiguration)
        {
            verintServiceGateway = _verintServiceGateway;
            verintConfiguration = _verintConfiguration.Value;
        }

        public async Task<string> CreateCase(ContactSTARTRequest request)
        {
            if (!verintConfiguration.ClassificationMap.TryGetValue(request.AreaOfConcern.Trim(), out var eventCode))
                throw new Exception("ContactSTARTService.CreateCase: EventCode not found");

            var response = await verintServiceGateway.CreateCase(request.MapToCase(eventCode));

            if (!response.IsSuccessStatusCode)
                throw new Exception($"ContactSTARTService.CreateCase: the status code {response.StatusCode} indicates something has gone wrong when attempting to create a case within verint-service.");

            return response.ResponseContent;
        }
    }
}
