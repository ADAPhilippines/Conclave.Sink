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

    private double _minimumHoneyValue;

    public double MinimumHoneyValue
    {
        get => _minimumHoneyValue;
        set
        {
            _minimumHoneyValue = value;
             OnPropertyChanged();
        }
    }

    private double _fromValue;

    public double FromValue
    {
        get => _fromValue;
        set
        {
            _fromValue = value;
             OnPropertyChanged();
        }
    }

    private double _toValue;

    public double ToValue
    {
        get => _toValue;
        set
        {
            _toValue = value;
             OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
