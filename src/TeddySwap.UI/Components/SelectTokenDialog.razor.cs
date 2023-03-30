using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Components;

public partial class SelectTokenDialog
{
    [Parameter]
    public IEnumerable<Token> Tokens { get; set; } = default!;

    private Token _selectedToken { get; set; } = default!;

    [Parameter]
    public EventCallback<Token> OnSelectedTokenClicked { get; set; }
    
    private string SearchValue { get; set; } = string.Empty;
}
