using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PojisteniApp2.Models;

namespace PojisteniApp2.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<PojisteniApp2.Models.Person> Person { get; set; } = default!;
        public DbSet<PojisteniApp2.Models.Insurance> Insurance { get; set; } = default!;
        public DbSet<PojisteniApp2.Models.InsuranceType> InsuranceType { get; set; } = default!;
    }
}