using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Components;

public partial class SelectTokenDialogItem
{
    [Parameter]
    public Token Token { get; set; } = default!;

    [Parameter]
    public EventCallback<Token> OnSelectedTokenClicked { get; set; }
}
