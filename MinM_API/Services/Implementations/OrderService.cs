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
                    UserFirstName = getUser.UserFirstName,
                    UserLastName = getUser.UserLastName,
                    UserEmail = getUser.Email,
                    UserPhone = getUser.PhoneNumber,
                };

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(order.OrderNumber);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<ServiceResponse<long>> CreateUnauthorizedOrder(AddOrderDto addOrderDto)
        {
            try
            {
                // Перевіряємо чи адреса вже існує
                var address = await context.Address.FirstOrDefaultAsync(a =>
                    a.Street == addOrderDto.Address.Street &&
                    a.HomeNumber == addOrderDto.Address.HomeNumber &&
                    a.City == addOrderDto.Address.City &&
                    a.Region == addOrderDto.Address.Region &&
                    a.PostalCode == addOrderDto.Address.PostalCode &&
                    a.Country == addOrderDto.Address.Country);

                // Якщо адреса не знайдена — створити і додати
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

                // Створюємо OrderItems (асинхронно, наприклад, з перевіркою чи товари існують)
                var orderItems = await CreateOrderItems(addOrderDto.OrderItems);

                var order = new Order
                {
                    Id = Guid.NewGuid().ToString(),
                    OrderDate = DateTime.UtcNow,
                    AddressId = address.Id,   // зв’язок по Id 
                    Address = address,
                    OrderItems = orderItems,
                    Status = Status.Created,
                    PaymentMethod = addOrderDto.PaymentMethod ?? "Card",
                    DeliveryMethod = addOrderDto.DeliveryMethod ?? "NovaPost",
                    OrderNumber = GenerateOrderNumber(),
                    // Всі user-поля для гостя
                    UserId = null,
                    User = null,
                    UserFirstName = addOrderDto.UserFirstName ?? "",
                    UserLastName = addOrderDto.UserLastName ?? "",
                    UserEmail = addOrderDto.UserEmail ?? "",
                    UserPhone = addOrderDto.UserPhone ?? ""
                };

                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                return ResponseFactory.Success(order.OrderNumber, "Order created successfully");
            }
            catch (Exception ex)
            {
                // Можна залогувати помилку
                Console.WriteLine($"Error in CreateUnauthorizedOrder: {ex.Message}");
                return ResponseFactory.Error<long>(0, "Internal error");
            }
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
