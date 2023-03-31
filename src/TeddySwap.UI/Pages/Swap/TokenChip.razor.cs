using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Pages.Swap;

public partial class TokenChip
{
    [Inject]
    IDialogService DialogService { get; set; } = default!;

    [Parameter]
    public IEnumerable<Token> Tokens { get; set; } = default!;

    private Token _currentlySelectedToken { get; set; } = default!;

    protected override void OnInitialized() =>  _currentlySelectedToken = Tokens.ElementAt(0);

    private void OpenSelectTokenDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("Tokens", Tokens);
        parameters.Add("OnSelectedTokenClicked", (Token token) => HandleSelectedToken(token));
        DialogService.Show<TokenSelectionDialog>("Select Token", parameters, options);
    }

    private void HandleSelectedToken(Token token) => _currentlySelectedToken = token;
}
