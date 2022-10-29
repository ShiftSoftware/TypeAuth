using TypeAuth.AspNetCore.Sample.Server.Data;
using TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces;

namespace TypeAuth.AspNetCore.Sample.Server.Repos
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly TypeAuthDbContext context;

        public UnitOfWork(TypeAuthDbContext context)
        {
            this.context = context;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await context.SaveChangesAsync();
        }
    }
}
