using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rocky.Models;
using Rocky_Models;

namespace Rocky.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Category { get; set; }
        public DbSet<ApplicationType> ApplicationType { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<InquiryHeader> InquiryHeader { get; set; }

        public DbSet<InquireDetail> InquireDetail { get; set; }

        public DbSet<OrderDetail> OrderDetail { get; set; }

        public DbSet<OrderHeader> OrderHeader { get; set; }

    }
}
