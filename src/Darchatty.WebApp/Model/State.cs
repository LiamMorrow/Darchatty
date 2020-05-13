using System.ComponentModel;

namespace Darchatty.WebApp.Model
{
    public class State : IState
    {
        private string? name;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Name
        {
            get => name;
            set
            {
                name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
    }
}
