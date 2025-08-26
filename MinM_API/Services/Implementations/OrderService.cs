using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Order;
using MinM_API.Extension;
using MinM_API.Mappers;
using MinM_API.Models;
using MinM_API.Repositories.Interfaces;
using MinM_API.Services.Interfaces;
using System.Security.Claims;

namespace MinM_API.Services.Implementations
{
    public class OrderService(DataContext context, IUserRepository userRepository, OrderItemMapper mapper) : IOrderService
    {
        public async Task<ServiceResponse<long>> CreateOrder(AddOrderDto addOrderDto, ClaimsPrincipal user)
        {
            var getUser = await userRepository.FindUser(user, context);
            if (getUser == null)
            {
                return ResponseFactory.Error<long>(0, "No users found");
            }

            try
            {
                var address = await context.Address.FirstOrDefaultAsync(a => a.Street == addOrderDto.Address.Street &&
                a.HomeNumber == addOrderDto.Address.HomeNumber && a.City == addOrderDto.Address.City && a.Region == addOrderDto.Address.Region &&
                a.PostalCode == addOrderDto.Address.PostalCode && a.Country == addOrderDto.Address.Country);

                if (address == null)
                {
                    address = new Models.Address
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = getUser.Id,
                        Street = addOrderDto.Address.Street,
                        HomeNumber = addOrderDto.Address.HomeNumber,
                        City = addOrderDto.Address.City,
                        Region = addOrderDto.Address.Region,
                        PostalCode = addOrderDto.Address.PostalCode,
                        Country = addOrderDto.Address.Country
                    };
                }

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderDate = DateTime.UtcNow,
                    UserId = getUser.Id,
                    User = getUser,
                    Address = address,
                    OrderItems = await CreateOrderItems(addOrderDto.OrderItems),
                    Status = Status.Created,
                    PaymentMethod = addOrderDto.PaymentMethod,
                    DeliveryMethod = addOrderDto.DeliveryMethod,
                    OrderNumber = GenerateOrderNumber(),
                    RecipientFirstName = addOrderDto.RecipientFirstName,
                    RecipientLastName = addOrderDto.RecipientLastName,
                    RecipientEmail = addOrderDto.RecipientEmail,
                    RecipientPhone = addOrderDto.RecipientEmail,
                };

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(order.OrderNumber);
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error<long>(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<long>> CreateUnauthorizedOrder(AddOrderDto addOrderDto)
        {
            try
            {
                var address = await context.Address.FirstOrDefaultAsync(a =>
                    a.Street == addOrderDto.Address.Street &&
                    a.HomeNumber == addOrderDto.Address.HomeNumber &&
                    a.City == addOrderDto.Address.City &&
                    a.Region == addOrderDto.Address.Region &&
                    a.PostalCode == addOrderDto.Address.PostalCode &&
                    a.Country == addOrderDto.Address.Country);

                if (address == null)
                {
                    address = new Models.Address
                    {
                        Id = Guid.NewGuid().ToString(),
                        Street = addOrderDto.Address.Street,
                        HomeNumber = addOrderDto.Address.HomeNumber,
                        City = addOrderDto.Address.City,
                        Region = addOrderDto.Address.Region,
                        PostalCode = addOrderDto.Address.PostalCode,
                        Country = addOrderDto.Address.Country
                    };

                    await context.Address.AddAsync(address);
                    await context.SaveChangesAsync();
                }

                var orderItems = await CreateOrderItems(addOrderDto.OrderItems);

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderDate = DateTime.UtcNow,
                    AddressId = address.Id,
                    Address = address,
                    OrderItems = orderItems,
                    Status = Status.Created,
                    PaymentMethod = addOrderDto.PaymentMethod ?? "Card",
                    DeliveryMethod = addOrderDto.DeliveryMethod ?? "NovaPost",
                    OrderNumber = GenerateOrderNumber(),
                    UserId = null,
                    User = null,
                    RecipientFirstName = addOrderDto.RecipientFirstName ?? "",
                    RecipientLastName = addOrderDto.RecipientLastName ?? "",
                    RecipientEmail = addOrderDto.RecipientEmail ?? "",
                    RecipientPhone = addOrderDto.RecipientPhone ?? ""
                };

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(order.OrderNumber, "Order created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateUnauthorizedOrder: {ex.Message}");
                return ResponseFactory.Error<long>(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<long>> CancelOrder(ClaimsPrincipal user, string orderId)
        {
            var getUser = await userRepository.FindUser(user, context);
            if (getUser == null)
            {
                return ResponseFactory.Error<long>(0, "No users found");
            }

            try
            {
                var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order == null)
                {
                    return ResponseFactory.Error<long>(0, "No orders found");
                }

                //if (order.Status is not Status.Created && order.Status is not Status.Canceled)
                //{
                //    var returnResult = await ReturnOrder(order.Id);
                //}

                order.Status = Status.Canceled;

                await context.SaveChangesAsync();

                return ResponseFactory.Success<long>(1, "Order was canceled");
            }
            catch(Exception ex)
            {
                return ResponseFactory.Error<long>(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<long>> SetOrderAsPaid(string orderId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var order = await context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ResponseFactory.Error<long>(0, "No orders found");
                }

                foreach (var orderItem in order.OrderItems)
                {
                    var productVariant = orderItem.Item;

                    if (productVariant == null || productVariant.UnitsInStock < orderItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return ResponseFactory.Error<long>(0, $"Insufficient stock for product {orderItem.Item.Name}");
                    }
                }

                order.Status = Status.Paid;

                foreach (var orderItem in order.OrderItems)
                {
                    var productVariant = orderItem.Item;

                    productVariant.UnitsInStock -= orderItem.Quantity;

                    if (productVariant.UnitsInStock <= 0)
                    {
                        productVariant.IsStock = false;
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ResponseFactory.Success<long>(1, "Order was paid and stock updated");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error<long>(0, $"Error processing payment: {ex.Message}");
            }
        }


        public async Task<ServiceResponse<long>> ChangeOrderStatus(string orderId, Status status)
        {
            try
            {
                var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order == null)
                {
                    return ResponseFactory.Error<long>(0, "No orders found");
                }

                if (status == Status.Returned)
                {
                    var result = await ReturnOrder(orderId);

                    if (!result.IsSuccessful)
                    {
                        return result;
                    }
                }

                order.Status = status;

                await context.SaveChangesAsync();

                return ResponseFactory.Success<long>(1, "Order status was changed by admin");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error<long>(0, "Internal error");
            }
        }

        public async Task<ServiceResponse<List<OrderDto>>> GetAllOrders()
        {
            try
            {
                var orders = await context.Orders.ToListAsync();
                if (orders.Count == 0)
                {
                    return ResponseFactory.Success(new List<OrderDto>(), "No orders found");
                }

                var getOrders = new List<OrderDto>();

                foreach(var order in orders)
                {
                    getOrders.Add(new OrderDto
                    {
                        Id = order.Id,
                        AddressId = order.AddressId,
                        Address = new Dtos.Address
                        {
                            Street = order.Address.Street ?? "",
                            HomeNumber = order.Address.HomeNumber ?? "",
                            City = order.Address.City ?? "",
                            Region = order.Address.Region ?? "",
                            PostalCode = order.Address.PostalCode ?? "",
                            Country = order.Address.Country ?? "",
                        },
                        OrderItems = await MapAsync(order.OrderItems),
                        PaymentMethod = order.PaymentMethod,
                        DeliveryMethod = order.DeliveryMethod,
                        RecipientFirstName = order.RecipientFirstName,
                        RecipientLastName = order.RecipientLastName,
                        RecipientEmail = order.RecipientEmail,
                        RecipientPhone = order.RecipientPhone,
                    });
                }

                return ResponseFactory.Success(getOrders, "Success");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error<List<OrderDto>>(null, "Internal error");
            }
        }

        public async Task<ServiceResponse<List<OrderDto>>> GetAllUserOrders(ClaimsPrincipal user)
        {
            try
            {
                var getUser = await userRepository.FindUser(user, context);
                if (getUser == null)
                {
                    return ResponseFactory.Error<List<OrderDto>>(null, "No users found");
                }

                var orders = await context.Orders.Where(o => o.UserId == getUser.Id).ToListAsync();
                if (orders.Count == 0)
                {
                    return ResponseFactory.Success(new List<OrderDto>(), "No orders found");
                }

                var getOrders = new List<OrderDto>();

                foreach (var order in orders)
                {
                    getOrders.Add(new OrderDto
                    {
                        Id = order.Id,
                        AddressId = order.AddressId,
                        Address = new Dtos.Address
                        {
                            Street = order.Address.Street ?? "",
                            HomeNumber = order.Address.HomeNumber ?? "",
                            City = order.Address.City ?? "",
                            Region = order.Address.Region ?? "",
                            PostalCode = order.Address.PostalCode ?? "",
                            Country = order.Address.Country ?? "",
                        },
                        OrderItems = await MapAsync(order.OrderItems),
                        PaymentMethod = order.PaymentMethod,
                        DeliveryMethod = order.DeliveryMethod,
                        RecipientFirstName = order.RecipientFirstName,
                        RecipientLastName = order.RecipientLastName,
                        RecipientEmail = order.RecipientEmail,
                        RecipientPhone = order.RecipientPhone,
                    });
                }

                return ResponseFactory.Success(getOrders, "Success");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error<List<OrderDto>>(null, "Internal error");
            }
        }

        public async Task<ServiceResponse<OrderDto>> GetUserOrder(string orderId)
        {
            try
            {
                var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order is null)
                {
                    return ResponseFactory.Success(new OrderDto(), "No orders found");
                }
                    
                var getOrder = new OrderDto
                {
                    Id = order.Id,
                    AddressId = order.AddressId,
                    Address = new Dtos.Address
                    {
                        Street = order.Address.Street ?? "",
                        HomeNumber = order.Address.HomeNumber ?? "",
                        City = order.Address.City ?? "",
                        Region = order.Address.Region ?? "",
                        PostalCode = order.Address.PostalCode ?? "",
                        Country = order.Address.Country ?? "",
                    },
                    OrderItems = await MapAsync(order.OrderItems),
                    PaymentMethod = order.PaymentMethod,
                    DeliveryMethod = order.DeliveryMethod,
                    RecipientFirstName = order.RecipientFirstName,
                    RecipientLastName = order.RecipientLastName,
                    RecipientEmail = order.RecipientEmail,
                    RecipientPhone = order.RecipientPhone,
                };

                return ResponseFactory.Success(getOrder, "Success");
            }
            catch (Exception ex)
            {
                return ResponseFactory.Error<OrderDto>(null, "Internal error");
            }
        }

        private async Task<ServiceResponse<long>> ReturnOrder(string orderId)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var order = await context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Item)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    return ResponseFactory.Error<long>(0, "No orders found");
                }

                foreach (var orderItem in order.OrderItems)
                {
                    var productVariant = orderItem.Item;

                    if (productVariant == null || productVariant.UnitsInStock < orderItem.Quantity)
                    {
                        await transaction.RollbackAsync();
                        return ResponseFactory.Error<long>(0, $"Insufficient stock for product {orderItem.Item.Name}");
                    }
                }

                order.Status = Status.Returned;

                foreach (var orderItem in order.OrderItems)
                {
                    var productVariant = orderItem.Item;

                    productVariant.UnitsInStock += orderItem.Quantity;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ResponseFactory.Success<long>(1, "Order was returned and stock updated");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ResponseFactory.Error<long>(0, $"Error processing payment: {ex.Message}");
            }
        }

        private async Task<List<OrderItemDto>> MapAsync(List<OrderItem> orderItems)
        {
            var result = new List<OrderItemDto>();

            foreach (var item in orderItems)
            {
                result.Add(mapper.OrderItemToOrderItemDto(item));
            }

            return result;
        }

        private async Task<List<OrderItem>> CreateOrderItems(List<OrderItemDto> orderItems)
        {
            var result = new List<OrderItem>();
            var itemToAdd = new OrderItem();

            foreach (var item in orderItems)
            {
                itemToAdd = mapper.OrderItemDtoToOrderItem(item);
                itemToAdd.Id = Guid.NewGuid().ToString();

                result.Add(itemToAdd);
            }
            return result;
        }

        private static long GenerateOrderNumber()
        {
            var guid = Guid.NewGuid();
            var bytes = guid.ToByteArray();

            var result = BitConverter.ToInt64(bytes, 0);

            return Math.Abs(result);
        }
    }
}
