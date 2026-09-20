using MinM_API.Dtos.Order;
using MinM_API.Models;
using Riok.Mapperly.Abstractions;

namespace MinM_API.Mappers
{
    [Mapper]
    public partial class OrderItemMapper
    {
        [MapperIgnoreTarget(nameof(OrderItem.Id))]
        [MapperIgnoreTarget(nameof(OrderItem.OrderId))]
        [MapperIgnoreTarget(nameof(OrderItem.Order))]
        [MapperIgnoreTarget(nameof(OrderItem.Item))]
        [MapperIgnoreSource(nameof(OrderItemDto.ColorName))]
        [MapperIgnoreSource(nameof(OrderItemDto.ColorHex))]
        [MapperIgnoreTarget(nameof(OrderItem.ColorName))]
        [MapperIgnoreTarget(nameof(OrderItem.ColorHex))]
        public partial OrderItem OrderItemDtoToOrderItem(OrderItemDto orderItemDto);


        [MapperIgnoreSource(nameof(OrderItem.Id))]
        [MapperIgnoreSource(nameof(OrderItem.Order))]
        [MapperIgnoreSource(nameof(OrderItem.Item))]
        public partial OrderItemDto OrderItemToOrderItemDto(OrderItem orderItem);
    }
}
