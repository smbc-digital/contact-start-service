using contact_start_service.Models;
using System.Threading.Tasks;

namespace contact_start_service.Services
{
    public interface IContactSTARTService
    {
        Task<string> CreateCase(ContactSTARTRequest request);
    }
}
