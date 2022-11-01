using Microsoft.JSInterop;
using TypeAuth.AspNetCore.Sample.Client.Services.Interfaces;

namespace TypeAuth.AspNetCore.Sample.Client.Services
{
    public class ClipboardService : IClipboardService
    {
        private readonly IJSRuntime _jsInterop;
        public ClipboardService(IJSRuntime jsInterop)
        {
            _jsInterop = jsInterop;
        }
        public async Task CopyToClipboard(string text)
        {
            await _jsInterop.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}
