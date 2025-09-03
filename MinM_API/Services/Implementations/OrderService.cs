using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Dtos;
using MinM_API.Dtos.Address;
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
                Models.Address address = null;

                // Визначаємо тип адреси на основі DTO
                if (addOrderDto.PostAddress is not null)
                {
                    // Шукаємо існуючу PostAddress
                    address = await context.PostAddresses.FirstOrDefaultAsync(a =>
                        a.Country == addOrderDto.PostAddress.Country &&
                        a.City == addOrderDto.PostAddress.City &&
                        a.Region == addOrderDto.PostAddress.Region &&
                        a.PostDepartment == addOrderDto.PostAddress.PostDepartment);

                    if (address == null)
                    {
                        // Створюємо нову PostAddress
                        var newPostAddress = new PostAddress
                        {
                            Id = Guid.NewGuid().ToString(),
                            Country = addOrderDto.PostAddress.Country,
                            City = addOrderDto.PostAddress.City,
                            Region = addOrderDto.PostAddress.Region,
                            PostDepartment = addOrderDto.PostAddress.PostDepartment
                        };

                        await context.PostAddresses.AddAsync(newPostAddress);
                        address = newPostAddress;
                    }
                }
                else if (addOrderDto.UserAddress is not null)
                {
                    // Шукаємо існуючу UserAddress для цього користувача
                    address = await context.UserAddresses.FirstOrDefaultAsync(a =>
                        a.Country == addOrderDto.UserAddress.Country &&
                        a.City == addOrderDto.UserAddress.City &&
                        a.Region == addOrderDto.UserAddress.Region &&
                        a.Street == addOrderDto.UserAddress.Street &&
                        a.HomeNumber == addOrderDto.UserAddress.HomeNumber &&
                        a.PostalCode == addOrderDto.UserAddress.PostalCode &&
                        a.UserId == getUser.Id);

                    if (address == null)
                    {
                        // Створюємо нову UserAddress
                        var newUserAddress = new UserAddress
                        {
                            Id = Guid.NewGuid().ToString(),
                            Country = addOrderDto.UserAddress.Country,
                            City = addOrderDto.UserAddress.City,
                            Region = addOrderDto.UserAddress.Region,
                            Street = addOrderDto.UserAddress.Street,
                            HomeNumber = addOrderDto.UserAddress.HomeNumber,
                            PostalCode = addOrderDto.UserAddress.PostalCode,
                            UserId = getUser.Id,
                            User = getUser
                        };

                        await context.UserAddresses.AddAsync(newUserAddress);
                        address = newUserAddress;
                    }
                }
                else
                {
                    return ResponseFactory.Error<long>(0, "Invalid address type");
                }

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderDate = DateTime.UtcNow,
                    UserId = getUser.Id,
                    User = getUser,
                    AddressId = address.Id, // Встановлюємо ID адреси
                    Address = address,
                    OrderItems = await CreateOrderItems(addOrderDto.OrderItems),
                    Status = Status.Created,
                    PaymentMethod = addOrderDto.PaymentMethod,
                    DeliveryMethod = addOrderDto.DeliveryMethod,
                    OrderNumber = GenerateOrderNumber(),
                    AdditionalInfo = addOrderDto.AdditionalInfo,
                    RecipientFirstName = addOrderDto.RecipientFirstName,
                    RecipientLastName = addOrderDto.RecipientLastName,
                    RecipientEmail = addOrderDto.RecipientEmail,
                    RecipientPhone = addOrderDto.RecipientPhone // Виправив помилку: було RecipientEmail
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
                Address address = null;

                // Пошук існуючої адреси серед всіх типів адрес
                if (addOrderDto.PostAddress is not null)
                {
                    // Шукаємо існуючу PostAddress
                    address = await context.PostAddresses.FirstOrDefaultAsync(a =>
                        a.Country == addOrderDto.PostAddress.Country &&
                        a.City == addOrderDto.PostAddress.City &&
                        a.Region == addOrderDto.PostAddress.Region &&
                        a.PostDepartment == addOrderDto.PostAddress.PostDepartment);

                    if (address == null)
                    {
                        // Створюємо нову PostAddress
                        var newPostAddress = new PostAddress
                        {
                            Id = Guid.NewGuid().ToString(),
                            Country = addOrderDto.PostAddress.Country,
                            City = addOrderDto.PostAddress.City,
                            Region = addOrderDto.PostAddress.Region,
                            PostDepartment = addOrderDto.PostAddress.PostDepartment
                        };

                        await context.PostAddresses.AddAsync(newPostAddress);
                        address = newPostAddress;
                    }
                }
                else if (addOrderDto.UserAddress is not null)
                {
                    // Шукаємо існуючу UserAddress (без прив'язки до користувача для гостьових замовлень)
                    address = await context.UserAddresses.FirstOrDefaultAsync(a =>
                        a.Country == addOrderDto.UserAddress.Country &&
                        a.City == addOrderDto.UserAddress.City &&
                        a.Region == addOrderDto.UserAddress.Region &&
                        a.Street == addOrderDto.UserAddress.Street &&
                        a.HomeNumber == addOrderDto.UserAddress.HomeNumber &&
                        a.PostalCode == addOrderDto.UserAddress.PostalCode &&
                        a.UserId == null); // Пошук серед адрес без користувача

                    if (address == null)
                    {
                        // Створюємо нову UserAddress без прив'язки до користувача
                        var newUserAddress = new UserAddress
                        {
                            Id = Guid.NewGuid().ToString(),
                            Country = addOrderDto.UserAddress.Country,
                            City = addOrderDto.UserAddress.City,
                            Region = addOrderDto.UserAddress.Region,
                            Street = addOrderDto.UserAddress.Street,
                            HomeNumber = addOrderDto.UserAddress.HomeNumber,
                            PostalCode = addOrderDto.UserAddress.PostalCode,
                            UserId = null, // Гостьове замовлення без користувача
                        };

                        await context.UserAddresses.AddAsync(newUserAddress);
                        address = newUserAddress;
                    }
                }
                else
                {
                    return ResponseFactory.Error<long>(0, "Invalid address type");
                }

                // Зберігаємо адресу, якщо вона нова
                await context.SaveChangesAsync();

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
                    AdditionalInfo = addOrderDto.AdditionalInfo,
                    UserId = null, // Гостьове замовлення
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

        public async Task<ServiceResponse<long>> FailOrder(string orderId)
        {
            try
            {
                var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                if (order == null)
                {
                    return ResponseFactory.Error<long>(0, "No orders found");
                }

                if (order.Status != Status.Created)
                {
                    return ResponseFactory.Error<long>(0, "Internal error");
                }

                order.Status = Status.Failed;

                await context.SaveChangesAsync();

                return ResponseFactory.Success<long>(1, "Something went wrong. Order was failed");
            }
            catch (Exception ex)
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
                // Завантажуємо замовлення з усіма потрібними навігаційними властивостями
                var orders = await context.Orders
                    .Include(o => o.Address)
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .ToListAsync();

                if (orders.Count == 0)
                {
                    return ResponseFactory.Success(new List<OrderDto>(), "No orders found");
                }

                var getOrders = new List<OrderDto>();

                foreach (var order in orders)
                {
                    // Мапінг OrderItems
                    var orderItemsDto = await MapAsync(order.OrderItems);

                    // Мапінг Address з урахуванням різних типів
                    AddressDto addressDto = null;
                    if (order.Address != null)
                    {
                        switch (order.Address)
                        {
                            case PostAddress postAddress:
                                addressDto = new PostAddressDto
                                {
                                    Country = postAddress.Country,
                                    City = postAddress.City,
                                    Region = postAddress.Region,
                                    PostDepartment = postAddress.PostDepartment
                                };
                                break;

                            case UserAddress userAddress:
                                addressDto = new UserAddressDto
                                {
                                    Country = userAddress.Country,
                                    City = userAddress.City,
                                    Region = userAddress.Region,
                                    Street = userAddress.Street,
                                    HomeNumber = userAddress.HomeNumber,
                                    PostalCode = userAddress.PostalCode
                                };
                                break;
                        }
                    }

                    var orderDto = new OrderDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        OrderDate = order.OrderDate,
                        AddressId = order.AddressId,
                        Address = addressDto,
                        OrderItems = orderItemsDto,
                        Status = order.Status.ToString(),
                        PaymentMethod = order.PaymentMethod,
                        DeliveryMethod = order.DeliveryMethod,
                        AdditionalInfo = order.AdditionalInfo,
                        RecipientFirstName = order.RecipientFirstName,
                        RecipientLastName = order.RecipientLastName,
                        RecipientEmail = order.RecipientEmail,
                        RecipientPhone = order.RecipientPhone,
                        UserName = order.User?.UserName
                    };

                    getOrders.Add(orderDto);
                }

                return ResponseFactory.Success(getOrders, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllOrders: {ex.Message}");
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

                // Завантажуємо замовлення користувача з усіма потрібними навігаційними властивостями
                var orders = await context.Orders
                    .Where(o => o.UserId == getUser.Id)
                    .Include(o => o.Address)
                    .Include(o => o.OrderItems)
                    .OrderByDescending(o => o.OrderDate) // Сортуємо за датою, найновіші спочатку
                    .ToListAsync();

                if (orders.Count == 0)
                {
                    return ResponseFactory.Success(new List<OrderDto>(), "No orders found");
                }

                var getOrders = new List<OrderDto>();

                foreach (var order in orders)
                {
                    // Мапінг OrderItems
                    var orderItemsDto = await MapAsync(order.OrderItems);

                    // Мапінг Address з урахуванням різних типів
                    AddressDto addressDto = null;
                    if (order.Address != null)
                    {
                        switch (order.Address)
                        {
                            case PostAddress postAddress:
                                addressDto = new PostAddressDto
                                {
                                    Country = postAddress.Country,
                                    City = postAddress.City,
                                    Region = postAddress.Region,
                                    PostDepartment = postAddress.PostDepartment
                                };
                                break;

                            case UserAddress userAddress:
                                addressDto = new UserAddressDto
                                {
                                    Country = userAddress.Country,
                                    City = userAddress.City,
                                    Region = userAddress.Region,
                                    Street = userAddress.Street,
                                    HomeNumber = userAddress.HomeNumber,
                                    PostalCode = userAddress.PostalCode
                                };
                                break;
                        }
                    }

                    var orderDto = new OrderDto
                    {
                        Id = order.Id,
                        OrderNumber = order.OrderNumber,
                        OrderDate = order.OrderDate,
                        AddressId = order.AddressId,
                        Address = addressDto,
                        OrderItems = orderItemsDto,
                        Status = order.Status.ToString(),
                        PaymentMethod = order.PaymentMethod,
                        DeliveryMethod = order.DeliveryMethod,
                        AdditionalInfo = order.AdditionalInfo,
                        RecipientFirstName = order.RecipientFirstName,
                        RecipientLastName = order.RecipientLastName,
                        RecipientEmail = order.RecipientEmail,
                        RecipientPhone = order.RecipientPhone,
                    };

                    getOrders.Add(orderDto);
                }

                return ResponseFactory.Success(getOrders, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAllUserOrders: {ex.Message}");
                return ResponseFactory.Error<List<OrderDto>>(null, "Internal error");
            }
        }

        public async Task<ServiceResponse<OrderDto>> GetUserOrder(string orderId)
        {
            try
            {
                // Завантажуємо замовлення з усіма потрібними навігаційними властивостями
                var order = await context.Orders
                    .Include(o => o.Address)
                    .Include(o => o.OrderItems)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order is null)
                {
                    return ResponseFactory.Error<OrderDto>(null, "No orders found");
                }

                // Мапінг OrderItems
                var orderItemsDto = await MapAsync(order.OrderItems);

                // Мапінг Address з урахуванням різних типів
                AddressDto addressDto = null;
                if (order.Address != null)
                {
                    switch (order.Address)
                    {
                        case PostAddress postAddress:
                            addressDto = new PostAddressDto
                            {
                                Country = postAddress.Country,
                                City = postAddress.City,
                                Region = postAddress.Region,
                                PostDepartment = postAddress.PostDepartment
                            };
                            break;

                        case UserAddress userAddress:
                            addressDto = new UserAddressDto
                            {
                                Country = userAddress.Country,
                                City = userAddress.City,
                                Region = userAddress.Region,
                                Street = userAddress.Street,
                                HomeNumber = userAddress.HomeNumber,
                                PostalCode = userAddress.PostalCode
                            };
                            break;
                    }
                }

                var getOrder = new OrderDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    OrderDate = order.OrderDate,
                    AddressId = order.AddressId,
                    Address = addressDto,
                    OrderItems = orderItemsDto,
                    Status = order.Status.ToString(),
                    PaymentMethod = order.PaymentMethod,
                    DeliveryMethod = order.DeliveryMethod,
                    AdditionalInfo = order.AdditionalInfo,
                    RecipientFirstName = order.RecipientFirstName,
                    RecipientLastName = order.RecipientLastName,
                    RecipientEmail = order.RecipientEmail,
                    RecipientPhone = order.RecipientPhone,
                    UserName = order.User?.UserName
                };

                return ResponseFactory.Success(getOrder, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserOrder: {ex.Message}");
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
