namespace contact_start_service.Models
{
    public class RefererPerson
    {
        public string Permissions { private get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string ConnectionAbout { get; set; }
        public bool HasPermissions => Permissions.Equals("yes");
    }
}