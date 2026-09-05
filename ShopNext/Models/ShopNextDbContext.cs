using Microsoft.EntityFrameworkCore;

namespace ShopNext.Models
{
    public class ShopNextDbContext : DbContext
    {
        public ShopNextDbContext(DbContextOptions<ShopNextDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<Shop> Shops { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Rider> Riders { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<OrderEvidence> OrderEvidences { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Coupon>().Property(c => c.DiscountValue).HasPrecision(18, 2);
            modelBuilder.Entity<Coupon>().Property(c => c.MinOrderAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Coupon>().Property(c => c.MaxDiscountAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Order>().Property(o => o.TotalAmount).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(i => i.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<OrderItem>().Property(i => i.TotalPrice).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(p => p.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(p => p.Mrp).HasPrecision(18, 2);
            modelBuilder.Entity<Product>().Property(p => p.Discount).HasPrecision(18, 2);
            modelBuilder.Entity<ProductVariant>().Property(v => v.Price).HasPrecision(18, 2);
            modelBuilder.Entity<Shop>().Property(s => s.Latitude).HasPrecision(18, 6);
            modelBuilder.Entity<Shop>().Property(s => s.Longitude).HasPrecision(18, 6);
            modelBuilder.Entity<Rider>().Property(r => r.CurrentLatitude).HasPrecision(18, 6);
            modelBuilder.Entity<Rider>().Property(r => r.CurrentLongitude).HasPrecision(18, 6);

            // Configure OrderEvidence relationship
            modelBuilder.Entity<OrderEvidence>()
                .HasOne(e => e.Order)
                .WithMany()
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Complaint relationships
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Customer)
                .WithMany()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Order)
                .WithMany()
                .HasForeignKey(c => c.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Offer relationship to Product
            modelBuilder.Entity<Offer>()
                .HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure CustomerAddress relationship
            modelBuilder.Entity<CustomerAddress>()
                .HasOne(a => a.Customer)
                .WithMany()
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Product relationship
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Shop)
                .WithMany()
                .HasForeignKey(p => p.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProductVariant relationship
            modelBuilder.Entity<ProductVariant>()
                .HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Order relationship to Customer (User)
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany()
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order relationship to Shop
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shop)
                .WithMany()
                .HasForeignKey(o => o.ShopId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Order relationship to Rider
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Rider)
                .WithMany()
                .HasForeignKey(o => o.RiderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure OrderItem relationship to Order
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure OrderItem relationship to Product
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany()
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Review relationships
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Customer)
                .WithMany()
                .HasForeignKey(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Shop)
                .WithMany()
                .HasForeignKey(r => r.ShopId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Wishlist relationships
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Customer)
                .WithMany()
                .HasForeignKey(w => w.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany()
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notification relationship
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Customer)
                .WithMany()
                .HasForeignKey(n => n.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}