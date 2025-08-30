using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MinM_API.Data;
using MinM_API.Models;
using MinM_API.Repositories.Implementations;
using MinM_API.Repositories.Interfaces;
using MinM_API.Services.Implementations;
using MinM_API.Services.Interfaces;
using MinM_API.Extension;
using MinM_API.Mappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MinM_API.Services.BackgroundServices;
using MinM_API.Dtos.Address;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    options.OperationFilter<Swashbuckle.AspNetCore.Filters.SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer("MyTokenScheme", options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["AuthToken"];
            if (!string.IsNullOrEmpty(token))
                context.Token = token;

            return Task.CompletedTask;
        }
    };

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddIdentityApiEndpoints<User>().AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager()
    .AddApiEndpoints();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")).UseLazyLoadingProxies());

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddControllers();

builder.Services.AddHostedService<DiscountExpirationService>();
builder.Services.AddHostedService<SeasonExpirationService>();
builder.Services.AddHostedService<CheckProductFreshnessService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IWishListService, WishListService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddSingleton<ProductMapper>();
builder.Services.AddSingleton<SeasonMapper>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<CategoryMapper>();
builder.Services.AddSingleton<DiscountMapper>();
builder.Services.AddSingleton<CartMapper>();
builder.Services.AddSingleton<OrderItemMapper>();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

    options.SerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
    {
        Modifiers = { AddAddressDtoModifier }
    };
});

static void AddAddressDtoModifier(JsonTypeInfo jsonTypeInfo)
{
    if (jsonTypeInfo.Type == typeof(AddressDto))
    {
        jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$type",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
            DerivedTypes =
            {
                new JsonDerivedType(typeof(PostAddressDto), "post"),
                new JsonDerivedType(typeof(UserAddressDto), "user")
            }
        };
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("NextJsCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

// Migrations with repeat attempts
app.MapGet("/health", () => Results.Ok("OK"));

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    int retryCount = 5;
    TimeSpan retryDelay = TimeSpan.FromSeconds(10);

    for (int i = 0; i < retryCount; i++)
    {
        try
        {
            dbContext.Database.Migrate();
            Console.WriteLine("Миграции успешно применены.");
            break; // Success - out of the loop
        }
        catch (Exception ex)
        {
            if (i < retryCount - 1)
            {
                Console.WriteLine($"Database unavailable, retrying in {retryDelay.TotalSeconds} seconds... Error: {ex.Message}");
                Thread.Sleep(retryDelay);
            }
            else
            {
                Console.WriteLine($"Failed to connect to database after {retryCount} attempts. Error: {ex.Message}");
                throw; // Rethrow after all retries are exhausted
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    // app.ApplyMigration();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await AdminExtension.SeedRolesAsync(services);
    await AdminExtension.SeedAdminAsync(services);
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors("NextJsCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapIdentityApi<User>();
app.MapControllers();


app.Run();
