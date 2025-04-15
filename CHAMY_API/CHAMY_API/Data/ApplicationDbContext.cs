using CHAMY_API.Models;
using Microsoft.EntityFrameworkCore;

namespace CHAMY_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ProductCategory> ProductCategory { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<PermissionRole> PermissionRoles { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Color> Colors { get; set; }
        public DbSet<Size> Sizes { get; set; }
        public DbSet<Sale> Sale { get; set; }
        public DbSet<History> History { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }  
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Product - Sale (1-n)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Sales)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.SaleId)
                .IsRequired(false); // SaleId có thể null

            modelBuilder.Entity<Comment>()
                 .HasOne(c => c.Customer)
                 .WithMany(cu => cu.Comments)
                 .HasForeignKey(c => c.CustomerId)
                 .OnDelete(DeleteBehavior.SetNull); // hoặc Restrict tùy bạn

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Product)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.ProductId);
            // Cấu hình khóa chính kép cho ProductCategory
            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            // Cấu hình mối quan hệ Product - ProductCategory
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategorys)
                .HasForeignKey(pc => pc.ProductId);

            // Cấu hình mối quan hệ Category - ProductCategory
            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategory)
                .HasForeignKey(pc => pc.CategoryId);

            // Cấu hình mối quan hệ Product - ProductImage (nếu chưa có)
            modelBuilder.Entity<ProductImage>()
                .HasKey(pi => new { pi.ProductId, pi.ImageId });

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Image)
                .WithMany(pi => pi.ProductImages)
                .HasForeignKey(pi => pi.ImageId);

            // Cấu hình khóa chính cho bảng liên kết UserPermission
            modelBuilder.Entity<UserPermission>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            // Cấu hình khóa chính cho bảng liên kết PermissionRole
            modelBuilder.Entity<PermissionRole>()
                .HasKey(pr => new { pr.PermissionId, pr.RoleId });

            // Cấu hình mối quan hệ cho UserPermission
            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId);

            // Cấu hình mối quan hệ cho PermissionRole
            modelBuilder.Entity<PermissionRole>()
                .HasOne(pr => pr.Permission)
                .WithMany(p => p.PermissionRoles)
                .HasForeignKey(pr => pr.PermissionId);

            modelBuilder.Entity<PermissionRole>()
                .HasOne(pr => pr.Role)
                .WithMany(r => r.PermissionRoles) 
                .HasForeignKey(pr => pr.RoleId);


            // Quan hệ CartItem - Product, Color, Size
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Color)
                .WithMany()
                .HasForeignKey(ci => ci.ColorId)
                .IsRequired(false);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Size)
                .WithMany()
                .HasForeignKey(ci => ci.SizeId)
                .IsRequired(false);

            modelBuilder.Entity<CartItem>()
                .HasOne(e => e.Customer)
                 .WithMany() // Nếu Customer không có collection CartItems
                 .HasForeignKey(e => e.CustomerId) // Chỉ định rõ CustomerId là khóa ngoại
                 .OnDelete(DeleteBehavior.SetNull); // Tùy chỉnh hành vi xóa
            // Quan hệ Order - OrderItem
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Orders)
                .HasForeignKey(oi => oi.OrderId);
            // Quan hệ OrderItem - Product, Color, Size
            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Color)
                .WithMany()
                .HasForeignKey(oi => oi.ColorId)
                .IsRequired(false);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(oi => oi.Size)
                .WithMany()
                .HasForeignKey(oi => oi.SizeId)
                .IsRequired(false);
            // Quan hệ Customer - Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.CartItems)
                .WithOne(ci => ci.Customer)
                .HasForeignKey(ci => ci.CustomerId);

            // Cấu hình mối quan hệ Customer - Order
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Orders)
                .WithOne(o => o.Customer) // Nếu Order không có navigation property ngược lại
                .HasForeignKey(o => o.CustomerId)
                .IsRequired(false);

            // Cấu hình mối quan hệ Customer - Comment (nếu cần)
            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Comments)
                .WithOne(co => co.Customer) // Điều chỉnh nếu Comment có navigation property ngược lại
                .HasForeignKey(c => c.CustomerId); // Thay "CustomerId" bằng tên khóa ngoại thực tế trong Comment
            // Cấu hình quan hệ giữa Customer và History
            modelBuilder.Entity<History>()
                .HasOne(h => h.Customer)
                .WithMany(c => c.History)
                .HasForeignKey(h => h.CustomerId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa History khi Customer bị xóa

            // Configure ProductColor (many-to-many)
            modelBuilder.Entity<ProductColor>()
                .HasKey(pc => new { pc.ProductId, pc.ColorId });

            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductColors)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<ProductColor>()
                .HasOne(pc => pc.Color)
                .WithMany(c => c.ProductColors)
                .HasForeignKey(pc => pc.ColorId);

            // Configure ProductSize (many-to-many)
            modelBuilder.Entity<ProductSize>()
                .HasKey(ps => new { ps.ProductId, ps.SizeId });

            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.ProductSizes)
                .HasForeignKey(ps => ps.ProductId);

            modelBuilder.Entity<ProductSize>()
                .HasOne(ps => ps.Size)
                .WithMany(s => s.ProductSizes)
                .HasForeignKey(ps => ps.SizeId);
            //
            modelBuilder.Entity<ProductVariant>()
               .HasIndex(v => new { v.ProductId, v.ColorId, v.SizeId })
               .IsUnique();
        }
    }
}