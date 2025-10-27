using ECommerce.DataAccess.EntityConfigurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using ECommerce.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        public DbSet<Brand> Brands { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ProductSubImage> ProductSubImages { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<ProductColor>()
            //    .HasKey(e => new { e.ProductId, e.Color });

            //modelBuilder.Entity<ProductSubImage>()
            //    .HasKey(e => new { e.ProductId, e.Img });

            //new ProductColorEntityTypeConfiguration().Configure(modelBuilder.Entity<ProductColor>());
            //new ProductImgEntityTypeConfiguration().Configure(modelBuilder.Entity<ProductSubImage>());

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductColorEntityTypeConfiguration).Assembly);

            base.OnModelCreating(modelBuilder);
        }
        public DbSet<ECommerce.ViewModels.NewPasswordVM> NewPasswordVM { get; set; } = default!;

        // Deprecated 

        //public ApplicationDbContext()
        //{
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);

        //    optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=ECommerce519;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        //}

       
    }
}
