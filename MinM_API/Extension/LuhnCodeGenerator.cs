namespace MinM_API.Extension
{
    public static class LuhnCodeGenerator
    {
        private static readonly Random _random = new();

        public static string Generate6DigitCode()
        {
            var digits = Enumerable.Range(0, 5)
                .Select(_ => _random.Next(0, 10))
                .ToList();

            int checkDigit = CalculateLuhnCheckDigit(digits);
            digits.Add(checkDigit);

            return string.Concat(digits);
        }

        private static int CalculateLuhnCheckDigit(List<int> digits)
        {
            var sum = 0;
            var isEven = true;

            for (int i = digits.Count - 1; i >= 0; i--)
            {
                var d = digits[i];
                if (isEven)
                {
                    d *= 2;
                    if (d > 9) d -= 9;
                }
                sum += d;
                isEven = !isEven;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit;
        }
    }

}
