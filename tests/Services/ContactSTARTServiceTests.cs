using contact_start_service.Controllers;
using contact_start_service.Models;
using contact_start_service.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace contact_start_service_tests.Services
{
    public class ContactSTARTServiceTests
    {
        private readonly ContactSTARTService _service;

        public ContactSTARTServiceTests()
        {
            _service = new ContactSTARTService();
        }

    }
}
