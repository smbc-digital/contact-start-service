using StockportGovUK.NetStandard.Models.Addresses;
using System;

namespace contact_start_service.Models
{
    public class RefereePerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TimeSlot { get; set; }
        public Address Address { get; set; }
    }
}