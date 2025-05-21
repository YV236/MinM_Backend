using MinM_API.Dtos.Discount;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class DiscountMapper
    {
        public partial GetDiscountDto DiscountToDiscountDto(Discount discount);

        public partial void UpdateDiscountToDiscount(UpdateDiscountDto discountDto, Discount discount);
    }
}
