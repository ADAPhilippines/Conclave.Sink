using MudBlazor;
using MudBlazor.Utilities;

namespace Conclave.Dashboard.Web;

public class ConclaveTheme : MudTheme
{
    public ConclaveTheme()
    {
        Palette = new Palette()
        {
            Primary = new MudColor("#F00"),
            Warning = new MudColor("A98E32"),
            Tertiary = new MudColor("rgba(65, 251, 251, 0.1)")
        };
    }
}