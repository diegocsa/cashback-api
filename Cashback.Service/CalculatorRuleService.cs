using Cashback.Domain.Interfaces.Service;
using Cashback.Domain.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Cashback.Service
{
    public class CalculatorRuleService : ICalculatorRuleService
    {
        private ILogger<CalculatorRuleService> _logger;
        public IEnumerable<CalculatorRuleItemModel> CashbackRanges { get; private set; }

        public CalculatorRuleService(ILoggerFactory logFactory)
        {
            _logger = logFactory.CreateLogger<CalculatorRuleService>();
            CashbackRanges = PopulateRanges();
        }

        private IEnumerable<CalculatorRuleItemModel> PopulateRanges()
        {
            yield return new CalculatorRuleItemModel()
            {
                Start = 0,
                End = 1000,
                Percentual = 10
            };

            yield return new CalculatorRuleItemModel()
            {
                Start = 1000,
                End = 1500,
                Percentual = 15
            };

            yield return new CalculatorRuleItemModel()
            {
                Start = 1500,
                Percentual = 20
            };
        }

        public decimal CalculateCashbackPercentual(decimal totalByPeriod)
        {
            var percentual = CashbackRanges.Where(x => totalByPeriod >= x.Start && (totalByPeriod < x.End || !x.End.HasValue)).Single().Percentual;
            _logger.LogDebug($"[CalculatorRuleService/CalculateCashbackPercentual] Calculado {percentual}% para o total {totalByPeriod}");
            return percentual;
        }

    }
}
