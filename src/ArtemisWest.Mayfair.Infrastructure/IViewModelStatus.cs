using System.ComponentModel;

namespace ArtemisWest.Mayfair.Infrastructure
{
    public interface IViewModelStatus : INotifyPropertyChanged
    {
        ViewModelState Status { get; }    
    }
}