using MudBlazor;
using MudBlazor.Utilities;

namespace Conclave.Dashboard.Web;

public class ConclaveTheme : MudTheme
{
    public ConclaveTheme()
    {
        Palette = new Palette()
        {
            Primary = Colors.Blue.Accent3, // normal pool card
            Secondary = Colors.Blue.Accent3, // normal pool button
            SecondaryDarken ="#2962FF",
            Tertiary = Colors.Lime.Darken2, // conclave pool button
            TertiaryDarken = "#9E9D24",
            Warning = Colors.Lime.Darken2, // conclave pool card
            Info = Colors.Teal.Lighten1,
            Success = Colors.Green.Darken2,
            Error = Colors.Red.Default,
            SecondaryContrastText = Colors.Shades.White,
            TertiaryContrastText = Colors.Shades.White,
            InfoContrastText = new MudColor("rgba(37, 19, 109, 1)"),
            Surface = new MudColor("rgba(37, 19, 109, 0.1)"), // pagination color
            LinesInputs = Colors.Indigo.Darken4,
            PrimaryContrastText = Colors.Indigo.Darken4,

            TextPrimary = Colors.Grey.Darken4,
            TextSecondary = Colors.Grey.Darken1,
            Background = Colors.Shades.White,
            LinesDefault = new MudColor("rgba(37, 19, 109, 0.1)"),

        };

        PaletteDark = new PaletteDark()
        {
            Primary = Colors.Blue.Accent3, // normal pool card
            Secondary = new MudColor("rgba(41, 121, 255, 0.1)"), // normal pool button
            SecondaryDarken ="#2979FF",
            Tertiary = new MudColor("rgba(41, 121, 255, 0.1)"), //conclave pool button
            TertiaryDarken = "#2979FF",
            Warning = Colors.Lime.Darken3, // conclave pool card
            Info = Colors.Teal.Lighten1,
            Success = Colors.Green.Darken2,
            Error = Colors.Red.Darken4,
            SecondaryContrastText = Colors.Shades.White,
            TertiaryContrastText = Colors.Shades.Black,
            InfoContrastText = Colors.Shades.White,
            // Surface = new MudColor("rgba(255, 255, 255, 0.5)"),
            Surface = new MudColor("rgba(65, 251, 251, 0.1)"),
            LinesInputs = Colors.Shades.White,
            PrimaryContrastText = Colors.Teal.Lighten1,


            TextPrimary = Colors.Shades.White,
            TextSecondary = Colors.Grey.Darken2,
            Background = Colors.Grey.Darken4,
            LinesDefault = Colors.Teal.Lighten1,

        };
    }
}