using Cashback.Domain;
using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Repository;
using Cashback.Domain.Interfaces.Service;
using Cashback.Domain.Model;
using Cashback.Infra.CrossCutting.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Cashback.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ILogger<AuthenticationService> _logger;
        private readonly IResellerRepository _repository;
        private readonly IConfiguration _configuration;

        public AuthenticationService(IResellerRepository repository, IConfiguration configuration, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<AuthenticationService>();
            _repository = repository;
            _configuration = configuration;
        }

        public Task<string> Login(LoginModel model)
        {
            try
            {
                var hashedPassword = Hasher.CreatePasswordHash(model.Email, model.Password);
                var reseller = _repository.Retrieve(x => x.Email == model.Email).SingleOrDefault();

                if (reseller == null || reseller.Password != hashedPassword)
                    throw new CashbackServiceException(Messages.LoginOrPassInvalid);

                _logger.LogInformation($"[AuthenticationService/Login] Token requisitado para {model.Email}");
                return Task.FromResult(Jwt.GenerateToken(_configuration["TokenJWT:Issuer"], _configuration["TokenJWT:Audience"], DateTime.Now.AddMinutes(int.Parse(_configuration["TokenJWT:DurationMinutes"])), _configuration["TokenJWT:Key"]));
            }
            catch (Exception ex)
            {
                _logger.LogError($"[AuthenticationService/Login] {ex.Message}", ex);
                throw ex;
            }

        }


    }
}
