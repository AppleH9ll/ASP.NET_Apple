using AirShop.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace AirShop.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<ProductReservation> ProductReservations => Set<ProductReservation>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.ParentId).HasColumnName("parent_id");
            e.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId);
        });

        mb.Entity<Product>(e =>
        {
            e.ToTable("products");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
            e.Property(x => x.Stock).HasColumnName("stock");
            e.Property(x => x.CategoryId).HasColumnName("category_id");
            e.Property(x => x.VolumeLiters).HasColumnName("volume_liters").HasPrecision(6, 2);
            e.Property(x => x.OriginRegion).HasColumnName("origin_region");
            e.Property(x => x.ImageUrl).HasColumnName("image_url");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at"); // Убрали HasColumnType
            e.HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
        });

        mb.Entity<Cart>(e =>
        {
            e.ToTable("carts");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.CreatedAt).HasColumnName("created_at"); // Убрали HasColumnType
            e.HasMany(x => x.Items).WithOne(x => x.Cart).HasForeignKey(x => x.CartId);
        });

        mb.Entity<CartItem>(e =>
        {
            e.ToTable("cart_items");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.CartId).HasColumnName("cart_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Quantity).HasColumnName("quantity");
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        mb.Entity<Role>(e =>
        {
            e.ToTable("roles");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Description).HasColumnName("description");
        });

        mb.Entity<User>(e =>
        {
            e.ToTable("users");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Login).HasColumnName("login");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.FullName).HasColumnName("full_name");
            e.Property(x => x.Email).HasColumnName("email");
            e.Property(x => x.RoleId).HasColumnName("role_id");
            e.Property(x => x.IsActive).HasColumnName("is_active");
            e.Property(x => x.CreatedAt).HasColumnName("created_at"); // Убрали HasColumnType
            e.HasOne(x => x.Role).WithMany().HasForeignKey(x => x.RoleId);
        });

        mb.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.TotalAmount).HasColumnName("total_amount").HasPrecision(12, 2);
            e.Property(x => x.CreatedAt).HasColumnName("created_at"); // Убрали HasColumnType
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            e.HasMany(x => x.Items).WithOne(x => x.Order).HasForeignKey(x => x.OrderId);
        });

        mb.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.OrderId).HasColumnName("order_id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.Quantity).HasColumnName("quantity");
            e.Property(x => x.Price).HasColumnName("price").HasPrecision(10, 2);
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
        });

        mb.Entity<ProductReservation>(e =>
        {
            e.ToTable("product_reservations");
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ProductId).HasColumnName("product_id");
            e.Property(x => x.UserId).HasColumnName("user_id");
            e.Property(x => x.Quantity).HasColumnName("quantity");
            e.Property(x => x.Status).HasColumnName("status");
            e.Property(x => x.CreatedAt).HasColumnName("created_at"); // Убрали HasColumnType
            e.HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });
    }
}