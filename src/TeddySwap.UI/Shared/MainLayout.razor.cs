using MudBlazor;
using MudBlazor.Utilities;

namespace TeddySwap.UI.Shared;

public partial class MainLayout
{
    protected bool IsDrawerOpen { get; set; } = false;
    protected MudTheme Theme { get; set; } = new()
    {
        PaletteDark = new PaletteDark()
        {
            Primary = new MudColor("rgb(3, 105, 161)"),
            Secondary = new MudColor("rgb(3, 105, 161)"),
            SecondaryDarken = "rgb(3, 105, 161)",
            Surface = new MudColor("rgba(24, 24, 27, 0.7)"),
            BackgroundGrey = new MudColor("rgb(24, 24, 27)"),
            Tertiary = new MudColor("rgba(15, 23, 42, 0.5)"),
            TextPrimary = new MudColor("#FFFFFF"),
            TextSecondary = new MudColor("rgb(161,161,170)"),
            DrawerBackground = new MudColor("#1f394d"),
            DrawerText = new MudColor("#FFFFFF")
        },
        Typography = new Typography()
        {
            Default = new Default()
            {
                FontFamily = new string[] { "Montserrat", "sans-serif" }
            }
        }
    };

    protected void ToggleDrawer() => IsDrawerOpen = !IsDrawerOpen;
}