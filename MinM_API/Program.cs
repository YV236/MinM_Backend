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

builder.Services.AddIdentityApiEndpoints<User>().AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager()
    .AddApiEndpoints();

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).UseLazyLoadingProxies());

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddControllers();

builder.Services.AddHostedService<DiscountExpirationService>();
builder.Services.AddHostedService<SeasonExpirationService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IDiscountService, DiscountService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();
builder.Services.AddScoped<JwtTokenService>();

builder.Services.AddSingleton<ProductMapper>();
builder.Services.AddSingleton<SeasonMapper>();
builder.Services.AddSingleton<UserMapper>();
builder.Services.AddSingleton<CategoryMapper>();
builder.Services.AddSingleton<DiscountMapper>();

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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.ApplyMigration();
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
