using System;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using cwnu_mc_auth_server.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace cwnu_mc_auth_server.Services
{
    public interface IVerificationService
    {
        public string MakeNewVerificationRequest(string uuid, string name);
        public bool CheckVerificationRequest(string authcode, out VerificationRequestModel? model);
        public bool RequestVerificationToken(VerificationRequestModel model);
        public bool CompleteVerification(string token);
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
            _logger.LogInformation($"{model.PlayerUuid} : {model.PlayerName} => Request verification token ({model.VerificationToken})");

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

                _logger.LogInformation($"{uuid} : {name} => Create new verification request #{model.AuthCode}");
                return model.AuthCode;
            }
            else return modelCache.AuthCode;
            
        
        }

        bool IVerificationService.CompleteVerification(string token)
        {
            var tokenCache = (VerificationRequestModel?)_memoryCache.Get(token);
            if(tokenCache is null) return false;

            _logger.LogInformation($"{tokenCache.PlayerUuid} : {tokenCache.PlayerName} => Verification Completed");
            _memoryCache.Remove(tokenCache.VerificationToken ?? "");
            _memoryCache.Remove(tokenCache.PlayerUuid);
            _memoryCache.Remove(tokenCache.AuthCode);

            return true;
        }
    }
}
