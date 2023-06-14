using contact_start_service.Models;

namespace contact_start_service.Services
{
    public interface IContactSTARTService
    {
        Task<string> CreateCase(ContactSTARTRequest request);
    }
}
