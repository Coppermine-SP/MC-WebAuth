using cwnu_mc_auth_server.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Caching.Memory;

namespace cwnu_mc_auth_server.Services
{
    public interface IVerificationService
    {
        public string MakeNewVerificationRequest(string uuid);
        public bool CheckVerificationRequest(string authcode);
        public bool SendVerificationRequest();
    }

    public class VerificationService : IVerificationService {
        private readonly ILogger<VerificationService> _logger;
        private readonly IMemoryCache _memoryCache;

        public VerificationService(ILogger<VerificationService> logger, IMemoryCache cache) {
            _logger = logger;
            _memoryCache = cache;
        }

        bool IVerificationService.CheckVerificationRequest(string authcode)
        {
            var cache = _memoryCache.Get(authcode);

            return cache is not null;
        }

        string IVerificationService.MakeNewVerificationRequest(string uuid)
        {
            var uuidCache = (string?)_memoryCache.Get(uuid);
            var modelCache = (uuidCache is null) ? null : (VerificationRequestModel?)_memoryCache.Get(uuidCache);

            if (modelCache is null) {
                var model = new VerificationRequestModel(uuid);
                _memoryCache.Set(uuid, model.AuthCode, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));
                _memoryCache.Set(model.AuthCode, model, new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromMinutes(10)));

                _logger.LogInformation($"{uuid} => Create new verification request #{model.AuthCode}");
                return model.AuthCode;
            }
            else return modelCache.AuthCode;
            
        
        }

        bool IVerificationService.SendVerificationRequest()
        {
            throw new NotImplementedException();
        }
    }
}
