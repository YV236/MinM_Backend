namespace MinM_API.Extension
{
    public static class DiscountExtension
    {
        public static decimal CountDiscountPrice(decimal price, decimal discountPercentage)
        {
            var whole = Math.Floor(price);

            var fractional = price - whole;

            decimal discountedPrice = whole - (whole * (discountPercentage / 100));

            discountedPrice = Math.Floor(discountedPrice);

            return discountedPrice + fractional;
        }
    }
}
