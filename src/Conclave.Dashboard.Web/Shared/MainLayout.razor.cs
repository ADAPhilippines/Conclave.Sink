namespace Conclave.Dashboard.Web.Shared;

public partial class MainLayout
{
    private bool _isDrawerOpen;

    private void ToggleDrawer() => _isDrawerOpen = !_isDrawerOpen;

    private string _walletAddress { get; set; } = "addr_test1qrr86cuspxp7e3cnpcweye...";

    private string _walletAddress2 { get; set; } = "addr_test1qr...";

    private string GetActivePageRelativePath() => 
        NavigationManager.ToBaseRelativePath(NavigationManager.Uri);
}
