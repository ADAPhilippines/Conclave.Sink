using MudBlazor;
using MudBlazor.Utilities;

namespace Conclave.Dashboard.Web;

public class ConclaveTheme : MudTheme
{
    public ConclaveTheme()
    {
        Palette = new Palette()
        {
            Primary = Colors.Blue.Accent2, // normal pool card
            Secondary = Colors.Blue.Accent2, // normal pool button
            SecondaryDarken ="#2979FF",
            Tertiary = new MudColor("rgba(169, 142, 50, 1)"), // conclave pool button
            TertiaryDarken = "#826d26",
            Warning = new MudColor("rgba(169, 142, 50, 1)"), // conclave pool card
            Info = new MudColor("#35B5B5"),
            Success = new MudColor("#079F36"),
            Error = Colors.Red.Default,
            SecondaryContrastText = new MudColor("#FFFFFF"),
            TertiaryContrastText = new MudColor("#FFFFFF"),
            InfoContrastText = new MudColor("rgba(37, 19, 109, 1)"),
            Surface = Colors.Shades.White // pagination color
        };

        PaletteDark = new PaletteDark()
        {
            Primary = new MudColor("#259B9B"), // normal pool card
            Secondary = new MudColor("rgba(65, 251, 251, 0.1)"), // normal pool button
            SecondaryDarken ="#1b7272",
            Tertiary = new MudColor("rgba(65, 251, 251, 0.1)"), //conclave pool button
            TertiaryDarken = "#1b7272",
            Warning = new MudColor("rgba(169, 142, 50, 1)"), // conclave pool card
            Info = new MudColor("#35B5B5"),
            Success = new MudColor("#079F36"),
            Error = Colors.Red.Darken4,
            SecondaryContrastText = new MudColor("#41FBFB"),
            TertiaryContrastText = new MudColor("#41FBFB"),
            InfoContrastText = new MudColor("#FFFFFF"),
            Surface = new MudColor("rgba(255, 255, 255, 0.5)")
        };
    }
}