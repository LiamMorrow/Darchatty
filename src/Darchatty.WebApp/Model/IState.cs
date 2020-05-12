using System.ComponentModel;

namespace Darchatty.WebApp.Model
{
    public interface IState: INotifyPropertyChanged
    {
        string Name { get; set; }
    }
}
