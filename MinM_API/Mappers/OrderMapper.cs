using MinM_API.Dtos.Order;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class OrderItemMapper
    {
        [MapperIgnoreSource(nameof(OrderItem.Id))]
        [MapperIgnoreSource(nameof(OrderItem.Order))]
        [MapperIgnoreSource(nameof(OrderItem.Item))]
        public partial OrderItem OrderItemDtoToOrderItem(OrderItemDto orderItemDto);
    }
}
