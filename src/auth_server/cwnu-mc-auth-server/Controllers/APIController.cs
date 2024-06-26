using System;
using System.Configuration;
using cwnu_mc_auth_server.Contexts;
using cwnu_mc_auth_server.Models;
using cwnu_mc_auth_server.Services;
using Microsoft.AspNetCore.Mvc;

namespace cwnu_mc_auth_server.Controllers
{
    public class APIController : Controller
    {
        private readonly ILogger<APIController> _logger;
        private readonly IVerificationService _verificationService;
        private readonly ServerDBContext _dbContext;
        private readonly IConfiguration _config;
        private string? _serverSecret;
        private List<String>? _allowedIp;

        public APIController(ILogger<APIController> logger, IVerificationService verificationService,
            IConfiguration config, ServerDBContext context)
        {
            _logger = logger;
            _verificationService = verificationService;
            _config = config;
            _dbContext = context;
            _serverSecret = _config.GetValue<string>("serverSecret");
            _allowedIp = _config.GetSection("APIAllowedIPList").Get<List<string>>();

            if (_serverSecret is null || String.IsNullOrWhiteSpace(_serverSecret))
            {
                _logger.LogCritical("serverSecret is empty or not configured! Check appsettings.json.");
                throw new ConfigurationErrorsException();
            }
        }

        private string _getClientIP() => HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();

        [HttpPost]
        public IActionResult GetDept(string secret)
        {
            if (_allowedIp is not null && !_allowedIp.Contains(_getClientIP()))
            {
                _logger.LogCritical($"{_getClientIP()} (not allowed) => GetDept attempted. secret={secret}.");
                return Unauthorized();
            }

            if (secret != _serverSecret)
            {
                _logger.LogCritical($"{_getClientIP()} => GetDept attempted with invalid server secret!");
                return Unauthorized();
            }

            _logger.LogInformation($"{_getClientIP()} => GetDept List.");
            return new JsonResult(new VerificationViewModel()
            {
                Depts = _dbContext.Depts.ToList()
            });
        }

        [HttpPost]
        public IActionResult Authorize(string secret, string uuid, string name)
        {
            if (_allowedIp is not null && !_allowedIp.Contains(_getClientIP()))
            {
                _logger.LogCritical($"{_getClientIP()} (not allowed) => Authorize attempted. secret={secret}.");
                return Unauthorized();
            }

            if (secret != _serverSecret)
            {
                _logger.LogCritical($"{uuid} : {name} => Authorize attempted with invalid server secret! ({_getClientIP()})");
                return Unauthorized();
            }
            if (String.IsNullOrWhiteSpace(uuid)) return BadRequest();

            var query = _dbContext.Users.SingleOrDefault(x => x.Uuid == uuid);

            if (query is null)
            {
                _logger.LogInformation($"{uuid} : {name} => Unauthorized.");
                string code = _verificationService.MakeNewVerificationRequest(uuid, name);
                return new JsonResult(new AuthorizeResponseModel()
                {
                    IsAuthorized = false,
                    AuthCode = code
                });
            }
            else
            {
                _logger.LogInformation($"{uuid} : {name} => Authorized.");
                return new JsonResult(new AuthorizeResponseModel()
                {
                    IsAuthorized = true,
                    DeptId = query.DeptId
                });
            }
        }
    }
}
