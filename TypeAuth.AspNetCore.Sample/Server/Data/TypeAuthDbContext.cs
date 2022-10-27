using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TypeAuth.AspNetCore.Sample.Server.Models;

namespace TypeAuth.AspNetCore.Sample.Server.Data
{
    public class TypeAuthDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }


        public TypeAuthDbContext(DbContextOptions<TypeAuthDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
