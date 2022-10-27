using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TypeAuth.AspNetCore.Sample.Server.Models;

namespace TypeAuth.AspNetCore.Sample.Server.Data.Configurations
{
    public class RoleConfiigurations : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
            builder.HasIndex(x => x.Name).IsUnique();
            builder.Property(x => x.AccessTree).IsRequired(false);
        }
    }
}
