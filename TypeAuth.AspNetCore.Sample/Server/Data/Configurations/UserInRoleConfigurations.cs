using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TypeAuth.AspNetCore.Sample.Server.Models;

namespace TypeAuth.AspNetCore.Sample.Server.Data.Configurations
{
    public class UserInRoleConfigurations : IEntityTypeConfiguration<UserInRole>
    {
        public void Configure(EntityTypeBuilder<UserInRole> builder)
        {
            builder.HasKey(x => new { x.UserId, x.RoleId });

            builder.HasOne(x => x.User).WithMany(x => x.UserInRoles)
                .IsRequired().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Role).WithMany(x => x.UserInRoles)
               .IsRequired().HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
