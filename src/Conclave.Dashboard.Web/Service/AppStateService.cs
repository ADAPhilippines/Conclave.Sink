using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Conclave.Dashboard.Web.Services;

public class AppStateService : INotifyPropertyChanged
{
    private bool _isDarkMode = false;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            _isDarkMode = value;
            OnPropertyChanged();
        }
    }

    private bool _isDrawerOpen = false;
    public bool IsDrawerOpen
    {
        get => _isDrawerOpen;
        set
        {
            _isDrawerOpen = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
