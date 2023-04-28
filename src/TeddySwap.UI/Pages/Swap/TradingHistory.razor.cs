using Microsoft.AspNetCore.Components;
using TeddySwap.UI.Models;

namespace TeddySwap.UI.Pages.Swap;

public partial class TradingHistory
{
    private List<Transaction> Transactions = new List<Transaction>();

    protected override void OnInitialized()
    {
        Transactions = new()
        {
            new()
            {
                CreatedTime = "2023-04-24 22:09 GMT+8",
                Type = TransactionType.Buy,
                Price = 0.060559M,
                Input = 6_000,
                Output = 363.354469M,
                Owner = "addr1q8r86cuspxp7e3cnpcweyeenmsl46llt3h9k5ugg7cc4kn6u0nrh4agzpe7wlsr3rnyj0huzcu7fmuxrutcqs6td4tasjvq92l"
            },
            new()
            {
                CreatedTime = "2023-04-24 22:09 GMT+8",
                Type = TransactionType.Sell,
                Price = 0.060559M,
                Input = 6_000,
                Output = 363.354469M,
                Owner = "addr1q8r86cuspxp7e3cnpcweyeenmsl46llt3h9k5ugg7cc4kn6u0nrh4agzpe7wlsr3rnyj0huzcu7fmuxrutcqs6td4tasjvq92l"
            },
            new()
            {
                CreatedTime = "2023-04-24 22:09 GMT+8",
                Type = TransactionType.Buy,
                Price = 0.060559M,
                Input = 6_000,
                Output = 363.354469M,
                Owner = "addr1q8r86cuspxp7e3cnpcweyeenmsl46llt3h9k5ugg7cc4kn6u0nrh4agzpe7wlsr3rnyj0huzcu7fmuxrutcqs6td4tasjvq92l"
            },
            new()
            {
                CreatedTime = "2023-04-24 22:09 GMT+8",
                Type = TransactionType.Sell,
                Price = 0.060559M,
                Input = 6_000,
                Output = 363.354469M,
                Owner = "addr1q8r86cuspxp7e3cnpcweyeenmsl46llt3h9k5ugg7cc4kn6u0nrh4agzpe7wlsr3rnyj0huzcu7fmuxrutcqs6td4tasjvq92l"
            },
            new()
            {
                CreatedTime = "2023-04-24 22:09 GMT+8",
                Type = TransactionType.Buy,
                Price = 0.060559M,
                Input = 6_000,
                Output = 363.354469M,
                Owner = "addr1q8r86cuspxp7e3cnpcweyeenmsl46llt3h9k5ugg7cc4kn6u0nrh4agzpe7wlsr3rnyj0huzcu7fmuxrutcqs6td4tasjvq92l"
            },
            new()
            {
                CreatedTime = "2023-04-24 22:09 GMT+8",
                Type = TransactionType.Buy,
                Price = 0.060559M,
                Input = 6_000,
                Output = 363.354469M,
                Owner = "addr1q8r86cuspxp7e3cnpcweyeenmsl46llt3h9k5ugg7cc4kn6u0nrh4agzpe7wlsr3rnyj0huzcu7fmuxrutcqs6td4tasjvq92l"
            }
        };
    }
}
