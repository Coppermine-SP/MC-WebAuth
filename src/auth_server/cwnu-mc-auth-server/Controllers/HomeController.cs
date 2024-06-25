using cwnu_mc_auth_server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using cwnu_mc_auth_server.Services;

namespace cwnu_mc_auth_server.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVerificationService _verificationService;

        public HomeController(ILogger<HomeController> logger, IVerificationService verificationService)
        {
            _logger = logger;
            _verificationService = verificationService;
        }

        public IActionResult Index()
        {
            ViewBag.ErrorMessage = TempData["errMessage"];
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Verification(string authcode)
        {
            ViewBag.Authcode = authcode;
            if (!_verificationService.CheckVerificationRequest(authcode))
            {
                TempData["errMessage"] = "인증 코드가 만료되었거나 유효하지 않습니다.";
                return Redirect("Index");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}