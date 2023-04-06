using Microsoft.AspNetCore.Components;
using MudBlazor;
using TeddySwap.UI.Models;
using TeddySwap.UI.Services;

namespace TeddySwap.UI.Pages.Swap;

public partial class TokenChip
{
    [Inject]
    IDialogService DialogService { get; set; } = default!;

    [Inject]
    protected new AppStateService AppStateService { get; set; } = default!;

    [Parameter]
    public IEnumerable<Token> Tokens { get; set; } = default!;

    [Parameter]
    public Token CurrentlySelectedToken { get; set; } = default!;

    [Parameter]
    public EventCallback<Token> OnSelectedTokenClicked { get; set; } = default!;

    [Parameter]
    public string Id { get; set; } = string.Empty;

    [Parameter]
    public bool Disabled { get; set; } = false;

    private void OpenSelectTokenDialog()
    {
        var options = new DialogOptions { CloseOnEscapeKey = true };
        var parameters = new DialogParameters();
        parameters.Add("Tokens", Tokens);
        parameters.Add("OnSelectedTokenClicked", (Token token) => HandleSelectedToken(token));
        DialogService.Show<TokenSelectionDialog>("Select Token", parameters, options);
    }

    private void HandleSelectedToken(Token token)
    {
        if (Id == "1") AppStateService.FromCurrentlySelectedToken = token;
        if (Id == "2") AppStateService.ToCurrentlySelectedToken = token;
    }
}
