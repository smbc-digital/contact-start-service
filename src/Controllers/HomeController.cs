using contact_start_service.Models;
using contact_start_service.Services;
using Microsoft.AspNetCore.Mvc;
using StockportGovUK.AspNetCore.Attributes.TokenAuthentication;

namespace contact_start_service.Controllers
{
    [Produces("application/json")]
    [Route("api/v1/[Controller]")]
    [ApiController]
    [TokenAuthentication]
    public class HomeController : ControllerBase
    {
        private IContactSTARTService contactSTARTService;
        public HomeController(IContactSTARTService _contactSTARTService) =>
            contactSTARTService = _contactSTARTService;

        [HttpPost]
        public async Task<IActionResult> Post(ContactSTARTRequest model)
        {
            return Ok(await contactSTARTService.CreateCase(model));
        }
    }
}