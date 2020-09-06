using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Cashback.Service.Test
{
    public class CalculatorRuleServiceTest
    {
        private CalculatorRuleService _service;

        public CalculatorRuleServiceTest()
        {
            _service = new CalculatorRuleService(new NullLoggerFactory());
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 10)]
        [InlineData(999, 10)]
        [InlineData(1000, 15)]
        [InlineData(1001, 15)]
        [InlineData(1400, 15)]
        [InlineData(1499, 15)]
        [InlineData(1500, 20)]
        [InlineData(1501, 20)]
        [InlineData(1600, 20)]
        public void CalculateCashbackPercentual_Ranges_Percent(decimal totalPeriod, decimal expectedPercent)
        {
            var result = _service.CalculateCashbackPercentual(totalPeriod);
            Assert.Equal(expectedPercent, result);
        }

    }
}
