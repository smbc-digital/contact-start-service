using StockportGovUK.NetStandard.Models.Addresses;
using System;

namespace contact_start_service.Models
{
    public class ContactSTARTRequest
    {
        public string AboutYourSelfRadio { private get; set; }
        public string AreaOfConcern { get; set; }
        public string MoreInfomation { get; set; }
        public RefererPerson RefererPerson { get; set; }
        public RefereePerson RefereePerson { get; set; }
        public bool IsAboutSelf => AboutYourSelfRadio.Equals("yes");

    }
    public class RefererPerson
    {
        public string Permissions { private get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ConnectionAbout { get; set; }
        public bool HasPermissions => Permissions.Equals("yes");
    }

    public class RefereePerson
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string TimeSlot { get; set; }
        public Address Address { get; set;  }
    }
}
