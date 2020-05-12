using System.ComponentModel;

namespace Darchatty.WebApp.Model
{
    public class State : IState
    {
        private string name;

        public string Name { get => name; set { name = value; this.PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name))); } }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
