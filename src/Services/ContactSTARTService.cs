using contact_start_service.Models;
using System.Threading.Tasks;

namespace contact_start_service.Services
{
    public interface IContactSTARTService
    {
        Task<string> CreateCase(ContactSTARTRequest request);
    }

    public class ContactSTARTService : IContactSTARTService
    {
        /**
         * TODO:: add int/qa/stage/prod secrets
         * TODO:: the body of this function
         **/
        public Task<string> CreateCase(ContactSTARTRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}
