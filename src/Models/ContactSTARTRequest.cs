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
}