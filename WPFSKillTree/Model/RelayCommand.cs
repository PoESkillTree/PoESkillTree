using System;
using System.Diagnostics;
using System.Windows.Input;

namespace POESKillTree.Model
{
    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : base(_ => execute(), canExecute == null ? null : (Predicate<object>) (_ => canExecute()))
        {
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
            {
                return Equals(parameter, default(T)) || parameter is T;
            }
            // Null or other defaults *are* valid input but are not considered
            // of correct type ("parameter is T" is false if it's null).
            if (Equals(parameter, default(T)))
            {
                return _canExecute(default(T));
            }
            return parameter is T && _canExecute((T) parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T) parameter);
        }
    }
}
