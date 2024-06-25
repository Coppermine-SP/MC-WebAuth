using cwnu_mc_auth_server.Models;
using cwnu_mc_auth_server.Services;
using Microsoft.AspNetCore.Mvc;

namespace cwnu_mc_auth_server.Controllers
{
    public class APIController : Controller
    {
        private readonly ILogger<APIController> _logger;
        private readonly IVerificationService _verificationService;

        public APIController(ILogger<APIController> logger, IVerificationService verificationService)
        {
            _logger = logger;
            _verificationService = verificationService;
        }

        [HttpPost]
        public IActionResult Authorize(string uuid, string name)
        {
            if (String.IsNullOrWhiteSpace(uuid)) return BadRequest();

            string code = _verificationService.MakeNewVerificationRequest(uuid, name);
            return new JsonResult(new AuthorizeResponseModel()
            {
                IsAuthorized = false,
                AuthCode = code
            });
        }
    }
}
