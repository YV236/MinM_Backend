using System;
using System.Linq;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MinM_API.Extension
{
    public static class OrderNumberGenerator
    {
        private static readonly Random _random = new();

        public static string GenerateOrderNumber()
        {
            DateTime now = DateTime.Now;
            string prefix = $"{now.Year}{now.Month:D2}";

            if (prefix.Length > 6)
                prefix = prefix.Substring(0, 6);

            string orderNumber;
            bool isValid = false;

            do
            {
                int remainingDigits = 18 - prefix.Length;
                string randomPart = GenerateRandomDigits(remainingDigits);

                string baseNumber = prefix + randomPart;

                int checkDigit = CalculateLuhnCheckDigit(baseNumber);

                orderNumber = baseNumber + checkDigit;

                isValid = ValidateLuhnCheck(orderNumber);

            } while (!isValid || orderNumber.Length != 19);

            return orderNumber;
        }

        public static bool ValidateOrderNumber(string orderNumber)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = orderNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(orderNumber[i].ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                        n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        private static string GenerateRandomDigits(int length)
        {
            return new string(Enumerable.Range(0, length)
                .Select(_ => (char)('0' + _random.Next(0, 10)))
                .ToArray());
        }

        private static int CalculateLuhnCheckDigit(string number)
        {
            int sum = 0;
            bool alternate = true;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = number[i];

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return (10 - sum % 10) % 10;
        }

        private static bool ValidateLuhnCheck(string number)
        {
            int sum = 0;
            bool alternate = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int digit = int.Parse(number[i].ToString());

                if (alternate)
                {
                    digit *= 2;
                    if (digit > 9)
                        digit -= 9;
                }

                sum += digit;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }
    }
}

