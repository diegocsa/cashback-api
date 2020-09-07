using Cashback.Domain;
using Cashback.Domain.Entities;
using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Repository;
using Cashback.Domain.Interfaces.Service;
using Cashback.Domain.Model;
using Cashback.Infra.CrossCutting.Auth;
using Cashback.Infra.CrossCutting.Extensions;
using Cashback.Infra.CrossCutting.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nelibur.ObjectMapper;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Cashback.Service
{
    public class ResellerService : IResellerService
    {
        private ILogger<ResellerService> _logger;
        private IConfiguration _configuration;
        private IResellerRepository _resellerRepository;
        private HttpClient _httpClient;

        public ResellerService(IResellerRepository resellerRepository, IHttpClientFactory httpClient, IConfiguration configuration, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<ResellerService>();
            _configuration = configuration;
            _resellerRepository = resellerRepository;
            _httpClient = httpClient.CreateClient("balance-api");
        }


        private void Validate(NewResellerModel model)
        {
            if (model == null)
                throw new CashbackServiceException(Messages.NullObject);

            if (_resellerRepository.Retrieve(x => x.Email == model.Email).Any())
                throw new CashbackServiceException(Messages.ResellerEmailInUse);

            if (!Validators.EmailIsValid(model.Email))
                throw new CashbackServiceException(Messages.EmailInvalid);

            if (!Validators.CPFIsValid(model.CPF))
                throw new CashbackServiceException(Messages.CPFInvalid);

            if (_resellerRepository.Retrieve(x => x.CPF == model.CPF.ApplyCPFFormat()).Any())
                throw new CashbackServiceException(Messages.ResellerCPFInUse);

            if (string.IsNullOrEmpty(model.Name))
                throw new CashbackServiceException(Messages.NameIsObrigatory);

            if (string.IsNullOrEmpty(model.Password))
                throw new CashbackServiceException(Messages.PasswordIsObrigatory);

            if (model.Name.Length > 100)
                throw new CashbackServiceException(Messages.NameShouldBeLessThan100Chars);

        }

        public async Task<Reseller> Add(NewResellerModel model)
        {
            Validate(model);
            var reseller = TinyMapper.Map<Reseller>(model);
            reseller.Id = Guid.NewGuid();
            reseller.CPF = model.CPF.ApplyCPFFormat();
            reseller.Password = Hasher.CreatePasswordHash(model.Email, model.Password);
            await _resellerRepository.Add(reseller);
            _logger.LogDebug($"[ResellerService/Add] {model.Email} adicionado");
            return reseller;
        }

        public async Task<double> GetAccumulated(string cpf)
        {
            if (!Validators.CPFIsValid(cpf))
                throw new CashbackServiceException(Messages.CPFInvalid);

            if (!_resellerRepository.Retrieve(x => x.CPF == cpf.ApplyCPFFormat()).Any())
                throw new CashbackServiceException(Messages.ResellerNotFoundByCPF);

            HttpResponseMessage response;
            try
            {
                _logger.LogInformation($"[ResellerService/GetAccumulated] Consultando API Externa para acumulado e cashback para [{cpf}]");
                response = await _httpClient.GetAsync($"{_configuration["BalanceAPI:RelativeURL"]}?cpf={cpf}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ResellerService/GetAccumulated] Erro ao consultar API Externa", ex);
                throw new CashbackServiceException(Messages.ErrorOnAccessExternalAPI);
            }

            try
            {
                var value = Convert.ToDouble(JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result).body.credit);
                _logger.LogInformation($"[ResellerService/GetAccumulated] Resultado da API Externa para acumulado e cashback para [{value}]");
                return value;
            }
            catch (Exception)
            {
                throw new CashbackServiceException(Messages.ErrorOnConvertResult);
            }

        }
    }
}
