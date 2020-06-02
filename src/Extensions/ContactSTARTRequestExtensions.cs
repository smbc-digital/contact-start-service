using contact_start_service.Builders;
using contact_start_service.Models;
using StockportGovUK.NetStandard.Models.Verint;
using System.Diagnostics.CodeAnalysis;

namespace contact_start_service.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ContactSTARTRequestExtensions
    {
        public static Case MapToCase(this ContactSTARTRequest request, int eventCode)
        {
            var description = new DescriptionBuilder();

            if (request.RefererPerson != null && !request.IsAboutSelf)
                description
                    .Add("(Lagan) Referrer", new[] { request.RefererPerson.FirstName, request.RefererPerson.LastName })
                    .Add("Connection to the Referee", request.RefererPerson.ConnectionAbout)
                    .Add("Contact number", request.RefererPerson.PhoneNumber)
                    .Add(string.Empty);

            description
                .Add("Client")
                .Add("Name", new[] { request.RefereePerson.FirstName, request.RefereePerson.LastName })
                .Add("Date of Birth", request.RefereePerson.DateOfBirth.ToShortDateString());

            if (!string.IsNullOrEmpty(request.RefereePerson.EmailAddress))
                description.Add("Email", request.RefereePerson.EmailAddress);

            if(!string.IsNullOrEmpty(request.RefereePerson.PhoneNumber))
                description
                    .Add("Tel", request.RefereePerson.PhoneNumber)
                    .Add("Call Time", request.RefereePerson.TimeSlot);

            if (request.RefereePerson.Address.IsAutomaticallyFound)
                description.Add("Address", request.RefereePerson.Address.SelectedAddress);
            else
                description.Add("Address", new[] {
                    request.RefereePerson.Address.AddressLine1,
                    request.RefereePerson.Address.AddressLine2,
                    request.RefereePerson.Address.Town,
                    request.RefereePerson.Address.Postcode }, ", ");

            description.Add("Primary concern", request.AreaOfConcern);

            if(!string.IsNullOrEmpty(request.MoreInfomation))
                description.Add("Details", request.MoreInfomation);

            return new Case
            {
                Classification = $"Public Health > START > {request.AreaOfConcern.Trim()}",
                EventCode = eventCode,
                AssociatedWithBehaviour = AssociatedWithBehaviourEnum.Individual,
                RaisedByBehaviour = RaisedByBehaviourEnum.Individual,
                Customer = new Customer
                {
                    Forename = request.RefereePerson.FirstName,
                    Surname = request.RefereePerson.LastName,
                    Email = request.RefereePerson.EmailAddress,
                    Mobile = request.RefereePerson.PhoneNumber,
                    DateOfBirth = request.RefereePerson.DateOfBirth,
                    Address = new Address
                    {
                        AddressLine1 = request.RefereePerson.Address.AddressLine1,
                        AddressLine2 = request.RefereePerson.Address.AddressLine2,
                        AddressLine3 = request.RefereePerson.Address.Town,
                        Postcode = request.RefereePerson.Address.Postcode,
                        Reference = request.RefereePerson.Address.PlaceRef,
                        UPRN = request.RefereePerson.Address.PlaceRef
                    }
                },
                Description = description.Build(),
            };
        }
    }
}
