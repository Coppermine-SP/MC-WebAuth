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
            VerificationRequestModel? model;
            ViewBag.Authcode = authcode;
            if (!_verificationService.CheckVerificationRequest(authcode, out model) || model is null)
            {
                TempData["errMessage"] = "인증 코드가 만료되었거나 유효하지 않습니다.";
                return Redirect("Index");
            }

            if (model.VerificationToken is not null)
            {
                TempData["errMessage"] = "이미 인증 메일을 발송했습니다.";
                return Redirect("Index");
            }

            ViewBag.uuid = model.PlayerUuid;
            ViewBag.nickName = model.PlayerName;

            return View();
        }

        public IActionResult CompleteVerification(string token)
        {
            if (String.IsNullOrWhiteSpace(token)) return BadRequest();

            if (_verificationService.CompleteVerification(token)) return View("VerificationCompleted");
            else return View("VerificationFailed");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult RequestVerificationToken(string authcode, int dept, int studentId)
        {
            VerificationRequestModel model;
            if (!_verificationService.CheckVerificationRequest(authcode, out model) || model is null)
            {
                TempData["errMessage"] = "인증 코드가 만료되었거나 유효하지 않습니다.";
                return Redirect("Index");
            }

            if (model.VerificationToken is not null)
            {
                TempData["errMessage"] = "이미 인증 메일을 발송했습니다.";
                return Redirect("Index");
            }

            model.SetVerificationToken();
            model.DeptCode = dept;
            model.StudentId = studentId.ToString();
            _verificationService.RequestVerificationToken(model);

            return View("VerificationTokenSent");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}