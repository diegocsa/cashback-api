namespace Cashback.Domain.Interfaces.Service
{
    public interface ICalculatorRuleService
    {
        decimal CalculateCashbackPercentual(decimal totalByPeriod);
    }
}
