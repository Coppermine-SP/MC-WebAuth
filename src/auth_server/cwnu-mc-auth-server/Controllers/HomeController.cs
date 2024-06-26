using cwnu_mc_auth_server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using cwnu_mc_auth_server.Contexts;
using cwnu_mc_auth_server.Services;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace cwnu_mc_auth_server.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IVerificationService _verificationService;
        private readonly ServerDBContext _dbContext;

        public HomeController(ILogger<HomeController> logger, IVerificationService verificationService, ServerDBContext context)
        {
            _logger = logger;
            _verificationService = verificationService;
            _dbContext = context;
        }

        public IActionResult Index()
        {
            ViewBag.ErrorMessage = TempData["errMessage"];
            return View();
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("Verification")]
        public IActionResult Verification(string authcode)
        {
            VerificationRequestModel? model;
            ViewBag.Authcode = authcode;
            if (!_verificationService.CheckVerificationRequest(authcode, out model) || model is null)
            {
                TempData["errMessage"] = "인증 코드가 만료되었거나 유효하지 않습니다.";
                return RedirectToAction("Index");
            }

            if (model.VerificationToken is not null)
            {
                TempData["errMessage"] = "이미 인증 메일을 발송했습니다.";
                return RedirectToAction("Index");
            }

            ViewBag.uuid = model.PlayerUuid;
            ViewBag.nickName = model.PlayerName;

            var viewModel = new VerificationViewModel()
            {
                Depts = _dbContext.Depts.ToList()
            };
            return View(viewModel);
        }

        [Route("CompleteVerification")]
        public IActionResult CompleteVerification(string token)
        {
            if (String.IsNullOrWhiteSpace(token)) return BadRequest();

            if (_verificationService.CompleteVerification(_dbContext, token)) return View("VerificationCompleted");
            else return View("VerificationFailed");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        [Route("RequestVerificationToken")]
        public IActionResult RequestVerificationToken(string authcode, int dept, int studentId)
        {
            VerificationRequestModel model;
            if (!_verificationService.CheckVerificationRequest(authcode, out model) || model is null)
            {
                TempData["errMessage"] = "인증 코드가 만료되었거나 유효하지 않습니다.";
                return RedirectToAction("Index");
            }

            if (model.VerificationToken is not null)
            {
                TempData["errMessage"] = "이미 인증 메일을 발송했습니다.";
                return RedirectToAction("Index");
            }

            var usr = _dbContext.Users.SingleOrDefault(x => x.StudentId == Util.GetSHA256Hash(studentId.ToString()));
            var dpt = _dbContext.Depts.SingleOrDefault(x => x.DeptId == dept);

            if (usr is not null)
            {
                TempData["errMessage"] = $"이미 이 학번으로 재학생 인증을 받았습니다.";
                return RedirectToAction("Index");
            }

            if (dpt is null)
            {
                TempData["errMessage"] = $"올바르지 않은 학과 데이터";
                return RedirectToAction("Index");
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