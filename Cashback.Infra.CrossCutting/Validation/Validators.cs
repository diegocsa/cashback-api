using System.Text.RegularExpressions;

namespace Cashback.Infra.CrossCutting.Validation
{
    public class Validators
    {
        public static bool EmailIsValid(string email)
        {
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, @"^(([^<>()\[\]\\.,;:\s@""]+(\.[^<>()\[\]\\.,;:\s@""]+)*)|("".+""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$");
        }
        
        public static bool CPFIsValid(string cpf)
        {
            int[] validateFirstDigitDArray = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] validateSecondDigitArraigity = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            if (string.IsNullOrEmpty(cpf) || !Regex.IsMatch(cpf, @"^(\d{11}|\d{3}\.\d{3}\.\d{3}\-\d{2})"))
                return false;

            cpf = cpf.Replace(".", "")
                     .Replace("-", "");

            var cpfBase = cpf.Substring(0, 9);

            int stDigit = 0;
            for (int i = 0; i < 9; i++)
                stDigit += int.Parse(cpfBase[i].ToString()) * validateFirstDigitDArray[i];
            
            var modstDigit = stDigit % 11;
            stDigit = modstDigit < 2 ? 0 : 11 - modstDigit;

            cpfBase += stDigit.ToString();

            int ndDigit = 0;
            for (int i = 0; i < 10; i++)
                ndDigit += int.Parse(cpfBase[i].ToString()) * validateSecondDigitArraigity[i];

            var modNdDigit = ndDigit % 11;
            ndDigit = modNdDigit < 2 ? 0 : 11 - modNdDigit;

            cpfBase += ndDigit.ToString();

            return cpf == cpfBase;
        }
    }
}
