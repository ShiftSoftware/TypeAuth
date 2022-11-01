namespace TypeAuth.AspNetCore.Sample.Client.Services.Interfaces
{
    public interface IClipboardService
    {
        Task CopyToClipboard(string text);
    }
}