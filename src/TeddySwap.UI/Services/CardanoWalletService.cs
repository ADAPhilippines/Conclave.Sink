using Microsoft.JSInterop;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Services;

// Scoped Service
public class CardanoWalletService
{
    private readonly IJSRuntime _jsRuntime;

    public string? ConnectedAddress { get; set; }
    public CardanoWallet? ConnectedWallet { get; set; }

    public CardanoWalletService(IJSRuntime jSRuntime)
    {
        _jsRuntime = jSRuntime;
    }

    public async Task<IEnumerable<CardanoWallet>> GetAvailableWalletsAsync()
    {
        ArgumentNullException.ThrowIfNull(_jsRuntime);
        return await _jsRuntime.InvokeAsync<IEnumerable<CardanoWallet>>("CardanoWalletService.getWallets");
    }

    public async Task<bool> EnableAsync(CardanoWallet wallet)
    {
        ArgumentNullException.ThrowIfNull(_jsRuntime);
        bool result = await _jsRuntime.InvokeAsync<bool>("CardanoWalletService.enableAsync", wallet.Id);

        if (result)
        {
            ConnectedAddress = await GetAddressAsync();
            ConnectedWallet = wallet;
        }

        return result;
    }

    public async Task<string> GetAddressAsync()
    {
        ArgumentNullException.ThrowIfNull(_jsRuntime);
        return await _jsRuntime.InvokeAsync<string>("CardanoWalletService.getAddressAsync");
    }
}