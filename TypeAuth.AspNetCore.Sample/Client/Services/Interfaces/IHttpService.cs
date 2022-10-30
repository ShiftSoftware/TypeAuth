namespace TypeAuth.AspNetCore.Sample.Client.Services.Interfaces
{
    public interface IHttpService
    {
        Task<bool> DeleteAsync(string url);
        Task<TResult> GetAsync<TResult>(string url, object query = null);
        Task<TResult> PostAsync<TResult, TValue>(string url, TValue value);
        Task<TResult> PutAsync<TResult, TValue>(string url, TValue value);
    }
}