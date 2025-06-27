using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MinM_API.Models;
using System.Reflection.Emit;

namespace MinM_API.Data
{
    public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<User>(options)
    {
        public DbSet<User> Users { get; set; }

        public DbSet<Address> Address { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Discount> Discounts { get; set; }

        public DbSet<Season> Seasons { get; set; }

        public DbSet<NewsletterSubscription> NewsletterSubscriptions { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }

        public DbSet<ProductImage> ProductImages { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasMany(u => u.History)
                .WithOne(o => o.User)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<User>()
                .HasOne(u => u.Address)
                .WithOne(a => a.User)
                .HasForeignKey<Address>(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<User>()
                .HasMany(u => u.Cart)
                .WithMany(p => p.UsersWithThisProductInCart)
                .UsingEntity<Dictionary<string, object>>(
                "UserProducts",
                j => j.HasOne<Product>().WithMany().HasForeignKey("ProductId"),
                j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
                );

            builder.Entity<WishlistItem>()
                .HasOne(w => w.User)
                .WithMany(u => u.WishList)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.History)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Order>()
                .HasOne(o => o.Address)
                .WithMany()
                .HasForeignKey(o => o.AddressId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WishlistItem>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Product>()
                .HasOne(p => p.Discount)
                .WithMany(d => d.Products)
                .HasForeignKey(p => p.DiscountId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>()
                .HasOne(p => p.Season)
                .WithMany(d => d.Products)
                .HasForeignKey(p => p.SeasonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ProductImage>()
                .HasOne(p => p.Product)
                .WithMany(d => d.ProductImages)
                .HasForeignKey(p => p.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<Category>()
                .HasMany(c => c.Subcategories)
                .WithOne(c => c.ParentCategory)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Category>()
                .HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
            .IsUnique();

            builder.Entity<Product>()
                .HasMany(p => p.Colors)
                .WithMany(c => c.Products)
                .UsingEntity(j => j.ToTable("ProductColors"));

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(rt => rt.Id);

                entity.HasIndex(rt => rt.Token)
                      .IsUnique();

                entity.HasIndex(rt => rt.UserId);

                entity.HasIndex(rt => rt.ExpiryDate);

                entity.HasOne(rt => rt.User)
                      .WithMany(u => u.RefreshTokens)
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.Property(rt => rt.Token)
                      .HasMaxLength(500)
                      .IsRequired();
            });
        }
    }
}
