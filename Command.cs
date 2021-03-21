using System;
using System.Windows.Input;

namespace GrailedWPF
{
    public class Command : ICommand
    {
        private readonly Predicate<object> _canExecute;

        private readonly Action<object> _execute;

        public Command(Action<object> execute)
        {
            _execute = execute;
            _canExecute = null;
        }

        public Command(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute.Invoke(parameter);
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}