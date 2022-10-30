using System.Net.Http.Json;
using System.Net;
using TypeAuth.AspNetCore.Sample.Client.Extensions;
using TypeAuth.AspNetCore.Sample.Client.Services.Interfaces;

namespace TypeAuth.AspNetCore.Sample.Client.Services
{
    public class HttpService : IHttpService
    {
        private readonly HttpClient http;

        public HttpService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<TResult> PostAsync<TResult, TValue>(string url, TValue value)
        {
            using var response = await http.PostAsJsonAsync(url, value);

            if (response.StatusCode != HttpStatusCode.OK)
                return default;

            return await response.Content.ReadFromJsonAsync<TResult>();
        }

        public async Task<TResult> GetAsync<TResult>(string url, object query = null)
        {
            var queryString = query?.ToQueryString() ?? null;
            return await http.GetFromJsonAsync<TResult>(url + (queryString is not null ? "?" + queryString : ""));
        }

        public async Task<TResult> PutAsync<TResult, TValue>(string url, TValue value)
        {
            using var response = await http.PutAsJsonAsync(url, value);

            if (response.StatusCode != HttpStatusCode.OK)
                return default;

            return await response.Content.ReadFromJsonAsync<TResult>();
        }

        public async Task<bool> DeleteAsync(string url)
        {
            using var response = await http.DeleteAsync(url);

            if (response.StatusCode != HttpStatusCode.OK)
                return false;

            return true;
        }
    }
}
