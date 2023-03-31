using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;
using MudBlazor;

namespace TeddySwap.UI.Pages.Swap;

public partial class TokenSelectionDialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = default!;

    [Parameter]
    public IEnumerable<Token> Tokens { get; set; } = default!;

    [Parameter]
    public Action<Token> OnSelectedTokenClicked { get; set; } = default!;
    
    private string SearchValue { get; set; } = string.Empty;
}
