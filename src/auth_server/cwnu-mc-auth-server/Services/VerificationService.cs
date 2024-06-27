using MC_WebAuth.Contexts;
using MC_WebAuth.Models;
using Microsoft.Extensions.Caching.Memory;

namespace MC_WebAuth.Services
{
    public interface IVerificationService
    {
        public string MakeNewVerificationRequest(string uuid, string name);
        public bool CheckVerificationRequest(string authcode, out VerificationRequestModel? model);
        public bool RequestVerificationToken(VerificationRequestModel model);
        public bool CompleteVerification(ServerDBContext _context, string token);
        public void CancelVerification(VerificationRequestModel model);
    }

    public class VerificationService : IVerificationService {
        private readonly ILogger<VerificationService> _logger;
        private readonly IMemoryCache _memoryCache;

        public VerificationService(ILogger<VerificationService> logger, IMemoryCache cache) {
            _logger = logger;
            _memoryCache = cache;
        }

        bool IVerificationService.CheckVerificationRequest(string authcode, out VerificationRequestModel? model)
        {
            var cache = _memoryCache.Get(authcode);
            model = (VerificationRequestModel?)cache;
            return cache is not null;
        }

        bool IVerificationService.RequestVerificationToken(VerificationRequestModel model)
        {
            _logger.LogInformation($"{model.PlayerUuid} : {model.PlayerName} => Request verification token ({model.VerificationToken}).");

            if (model.VerificationToken is null) return false;
            _memoryCache.Set(model.VerificationToken, model, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));

            return true;
        }

        string IVerificationService.MakeNewVerificationRequest(string uuid, string name)
        {
            var uuidCache = (string?)_memoryCache.Get(uuid);
            var modelCache = (uuidCache is null) ? null : (VerificationRequestModel?)_memoryCache.Get(uuidCache);

            if (modelCache is null) {
                var model = new VerificationRequestModel(uuid, name);
                _memoryCache.Set(uuid, model.AuthCode, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                _memoryCache.Set(model.AuthCode, model, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));

                _logger.LogInformation($"{uuid} : {name} => Create new verification request #{model.AuthCode}.");
                return model.AuthCode;
            }
            else return modelCache.AuthCode;
        }

        void IVerificationService.CancelVerification(VerificationRequestModel model)
        {
            _logger.LogInformation($"#{model.AuthCode} ({model.VerificationToken}) => Verification Canceled.");
            _memoryCache.Remove(model.VerificationToken ?? "");
            _memoryCache.Remove(model.PlayerUuid);
            _memoryCache.Remove(model.AuthCode);
        }

        bool IVerificationService.CompleteVerification(ServerDBContext _context, string token)
        {
            var tokenCache = (VerificationRequestModel?)_memoryCache.Get(token);
            if(tokenCache is null) return false;


            var model = new User()
            {
                DeptId = tokenCache.DeptCode.GetValueOrDefault(1),
                StudentId = Util.GetSHA256Hash(tokenCache.StudentId),
                Uuid = tokenCache.PlayerUuid
            };

            _context.Users.Add(model);
            _context.SaveChanges();

            _logger.LogInformation($"{tokenCache.PlayerUuid} : {tokenCache.PlayerName} => Verification Completed.");
            _memoryCache.Remove(tokenCache.VerificationToken ?? "");
            _memoryCache.Remove(tokenCache.PlayerUuid);
            _memoryCache.Remove(tokenCache.AuthCode);

            return true;
        }
    }
}
