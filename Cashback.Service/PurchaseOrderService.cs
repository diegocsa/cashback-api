using Cashback.Domain;
using Cashback.Domain.Constants;
using Cashback.Domain.Entities;
using Cashback.Domain.Exceptions;
using Cashback.Domain.Interfaces.Repository;
using Cashback.Domain.Interfaces.Service;
using Cashback.Domain.Model;
using Cashback.Infra.CrossCutting.Extensions;
using Cashback.Infra.CrossCutting.Validation;
using Microsoft.Extensions.Logging;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cashback.Service
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private ILogger<PurchaseOrderService> _logger;
        private ICalculatorRuleService _calculatorRuleService;
        private IResellerRepository _resellerRepository;
        private IPurchaseOrderRepository _purchaseRepository;

        public PurchaseOrderService(IResellerRepository resellerRepository, IPurchaseOrderRepository purchaseRepository, ICalculatorRuleService calculatorRuleService, ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<PurchaseOrderService>();
            _calculatorRuleService = calculatorRuleService;
            _resellerRepository = resellerRepository;
            _purchaseRepository = purchaseRepository;
        }


        private void Validate(NewPurchaseOrderModel model)
        {
            if (model == null)
                throw new CashbackServiceException(Messages.NullObject);
            
            if (!Validators.CPFIsValid(model.CPF))
                throw new CashbackServiceException(Messages.ResellerCPFInvalid);

            if (GetReseller(model.CPF) == null)
                throw new CashbackServiceException(Messages.ResellerNotFoundByCPF);

            if (model.Value <= 0)
                throw new CashbackServiceException(Messages.PurchaseValueMustBeGreaterThanZero);
        }

        private Reseller GetReseller(string cpf)
        {
            var reseller = _resellerRepository.Retrieve(x => x.CPF == cpf.ApplyCPFFormat()).SingleOrDefault();
            return reseller;
        }

        public async Task<PurchaseOrder> Add(NewPurchaseOrderModel model)
        {
            Validate(model);
            var reseller = GetReseller(model.CPF);
            var status = reseller.AutoApproved ? Constants.PURCHASE_STATUS_APPROVED : Constants.PURCHASE_STATUS_WAITING_APPROVAL;
            var purchase = TinyMapper.Map<PurchaseOrder>(model);
            purchase.Id = Guid.NewGuid();
            purchase.Status = status;
            purchase.ResellerId = reseller.Id;
            await _purchaseRepository.Add(purchase);
            _logger.LogInformation($"[PurchaseOrderService/Add] Compra {purchase.Id} adicionada");
            return purchase;
        }

        public IEnumerable<PurchaseOrderWithCashbackValueModel> Get(string cpf, DateTime? start, DateTime? end)
        {
            _logger.LogDebug($"[PurchaseOrderService/Get] Relatorio de cashback para {cpf} no periodo {start:dd/MM/yyyy} a {end:dd/MM/yyyy}");

            if (!Validators.CPFIsValid(cpf))
                throw new CashbackServiceException(Messages.CPFInvalid);

            // É necessário pegar o mês completo para calcular o cashback corretamente;
            var startMonth = new DateTime(start.Value.Year, start.Value.Month, 1);
            var endMonth = new DateTime(end.Value.Year, end.Value.Month, 1).AddMonths(1).AddDays(-1);

            var items = _purchaseRepository
                            .Retrieve(x => x.Reseller.CPF == cpf.ApplyCPFFormat() && x.Date >= startMonth && x.Date < endMonth)
                            .OrderBy(x => x.Date)
                            .ToList();

            decimal accumulatedMonth = 0;
            if (items.Count == 0)
                yield return null;
            
            var previousDate = items.First().Date;
            foreach (var item in items)
            {
                if (previousDate.Year == item.Date.Year && previousDate.Month == item.Date.Month)
                {
                    accumulatedMonth += item.Value;
                }
                else
                {
                    accumulatedMonth = 0;
                    previousDate = item.Date;
                }

                var percent = _calculatorRuleService.CalculateCashbackPercentual(accumulatedMonth);
                var cashbackValue = item.Value * percent / 100;

                //utiliza o mes todo para recuperar o valor de cashback, porém só exibe as compras do range de datas
                if (item.Date.Date >= start.Value.Date && item.Date.Date <= end.Value.Date)
                    yield return new PurchaseOrderWithCashbackValueModel()
                    {
                        Id = item.Id,
                        Value = item.Value,
                        Date = item.Date,
                        CashbackPercentual = percent,
                        CashbackValue = cashbackValue,
                        Status = item.Status.SwitchStatusToDescription()
                    };
            }
        }
    }
}
