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
            Tertiary = Colors.Lime.Darken3, // conclave pool button
            TertiaryDarken = "#826d26",
            Warning = Colors.Lime.Darken3, // conclave pool card
            Info = Colors.Teal.Lighten1,
            Success = Colors.Green.Darken2,
            Error = Colors.Red.Default,
            SecondaryContrastText = Colors.Shades.White,
            TertiaryContrastText = Colors.Shades.White,
            InfoContrastText = new MudColor("rgba(37, 19, 109, 1)"),
            Surface = Colors.Shades.White, // pagination color
            Divider = Colors.Blue.Accent2,
            DividerLight = Colors.Lime.Darken3,
            LinesDefault = Colors.Blue.Darken4
        };

        PaletteDark = new PaletteDark()
        {
            Primary = Colors.Blue.Accent3, // normal pool card
            Secondary = new MudColor("rgba(41, 121, 255, 0.1)"), // normal pool button
            SecondaryDarken ="#2979FF",
            Tertiary = new MudColor("rgba(41, 121, 255, 0.1)"), //conclave pool button
            TertiaryDarken = "#2979FF",
            Warning = Colors.Lime.Darken4, // conclave pool card
            Info = Colors.Teal.Lighten3,
            Success = Colors.Green.Darken2,
            Error = Colors.Red.Darken4,
            SecondaryContrastText = Colors.Shades.White,
            TertiaryContrastText = Colors.Shades.White,
            InfoContrastText = Colors.Shades.White,
            Surface = new MudColor("rgba(255, 255, 255, 0.5)"),
            Divider = Colors.Blue.Accent2,
            DividerLight = Colors.Blue.Accent2
        };
    }
}