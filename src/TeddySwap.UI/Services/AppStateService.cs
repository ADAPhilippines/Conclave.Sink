using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TeddySwap.UI.Services;

public class AppStateService : INotifyPropertyChanged
{
    private double _slippageToleranceValue;

    public double SlippageToleranceValue
    {
        get => _slippageToleranceValue;
        set
        {
            _slippageToleranceValue = value;
             OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
