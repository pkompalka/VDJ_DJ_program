using System;
using System.Windows.Input;

namespace VDJServer.Utilities
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> execute;

        private readonly Predicate<object> canExecute;

        public RelayCommand(Action<object> exec, Predicate<object> canExec = null)
        {
            execute = exec;
            canExecute = canExec;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
