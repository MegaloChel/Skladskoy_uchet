using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace WpfApp22.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string _statusMessage = "Готово";

    public MainViewModel(Action refreshAction, Action buildReportAction)
    {
        RefreshCommand = new RelayCommand(refreshAction);
        BuildReportCommand = new RelayCommand(buildReportAction);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand BuildReportCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
