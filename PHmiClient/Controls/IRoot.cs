using System.Windows.Input;

namespace PHmiClient.Controls {
    public interface IRoot {
        object Content { get; set; }
        ICommand ShowCommand { get; }
        object HomePage { get; set; }
        ICommand HomeCommand { get; }
        ICommand BackCommand { get; }
        ICommand ForwardCommand { get; }
        ICommand LogOnCommand { get; }
        ICommand LogOffCommand { get; }
        ICommand ChangePasswordCommand { get; }

        void Show(object objOrType);

        void Show<T>();
    }
}