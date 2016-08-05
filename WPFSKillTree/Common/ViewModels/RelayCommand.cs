using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace POESKillTree.Common.ViewModels
{
    public class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : base(_ => execute(), canExecute == null ? null : (Predicate<object>) (_ => canExecute()))
        {
        }
    }

    public class AsyncRelayCommand : AsyncRelayCommand<object>
    {
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
            : base(_ => execute(), canExecute == null ? null : (Predicate<object>) (_ => canExecute()))
        {
        }
    }

    public abstract class AbstractRelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;

        protected AbstractRelayCommand([CanBeNull] Predicate<T> canExecute)
        {
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
            return parameter is T && _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            Execute((T) parameter);
        }

        protected abstract void Execute(T parameter);
    }

    public class RelayCommand<T> : AbstractRelayCommand<T>
    {
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
            : base(canExecute)
        {
            _execute = execute;
        }

        protected override void Execute(T parameter)
        {
            _execute(parameter);
        }
    }

    public class AsyncRelayCommand<T> : AbstractRelayCommand<T>
    {
        private readonly Func<T, Task> _execute;

        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null) : base(canExecute)
        {
            _execute = execute;
        }

        protected override async void Execute(T parameter)
        {
            await _execute(parameter);
        }

        public async Task ExecuteAsync(T parameter)
        {
            await _execute(parameter);
        }
    }
}
