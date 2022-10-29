namespace TypeAuth.AspNetCore.Sample.Server.Repos.Interfaces
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
    }
}