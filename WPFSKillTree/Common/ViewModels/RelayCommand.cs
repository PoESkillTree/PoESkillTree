using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace PoESkillTree.Common.ViewModels
{
    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates.
    /// </summary>
    public class RelayCommand : RelayCommand<object>
    {
        /// <param name="execute">The action that is called when this command is executed.</param>
        /// <param name="canExecute">A function returning whether this command can currently be executed.
        /// Null if all parameters can be executed.</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : base(_ => execute(), canExecute == null ? null : (Predicate<object>) (_ => canExecute()))
        {
        }
    }

    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates. The execution delegate is async, however, this is
    /// mostly for convenience as users of <see cref="ICommand"/>s can't wait on async execution
    /// (<see cref="ICommand.Execute"/> will return at the first await call). If it needs to be waited upon,
    /// <see cref="ExecuteAsync"/> can be used.
    /// </summary>
    public class AsyncRelayCommand : AsyncRelayCommand<object>
    {
        /// <param name="execute">The async action that is called when this command is executed.</param>
        /// <param name="canExecute">A function returning whether this command can currently be executed.
        /// Null if all parameters can be executed.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
            : base(_ => execute(), canExecute == null ? null : (Predicate<object>) (_ => canExecute()))
        {
        }

        /// <summary>
        /// Executes this command asynchronously and returns a task that finishes when the command is finished.
        /// </summary>
        public async Task ExecuteAsync()
        {
            await ExecuteAsync(null);
        }
    }

    /// <summary>
    /// Abstract <see cref="ICommand"/> implementation. Only leaves the execution for subclasses to implement.
    /// </summary>
    /// <typeparam name="T">The type of this command's parameter</typeparam>
    public abstract class AbstractRelayCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;

        /// <param name="canExecute">A predicate returning whether this command can be executed with a given parameter.
        /// Null if all parameters that are of correct type or <c>default(T)</c> can be executed.</param>
        protected AbstractRelayCommand([CanBeNull] Predicate<T> canExecute)
        {
            _canExecute = canExecute ?? (_ => true);
        }

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            // Null or other defaults *are* valid input but are not considered
            // of correct type ("parameter is T" is false if it's null).
            return (parameter is T || Equals(parameter, default(T))) && _canExecute((T)parameter);
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

        /// <summary>
        /// Executes this command with the given parameter.
        /// </summary>
        protected abstract void Execute(T parameter);
    }

    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates.
    /// </summary>
    /// <typeparam name="T">The type of this command's parameter</typeparam>
    public class RelayCommand<T> : AbstractRelayCommand<T>
    {
        private readonly Action<T> _execute;

        /// <param name="execute">The action that is called when this command is executed.</param>
        /// <param name="canExecute">A predicate returning whether this command can be executed with a given parameter.
        /// Null if all parameters that are of correct type or <c>default(T)</c> can be executed.</param>
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

    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates. The execution delegate is async, however, this is
    /// mostly for convenience as users of <see cref="ICommand"/>s can't wait on async execution
    /// (<see cref="ICommand.Execute"/> will return at the first await call). If it needs to be waited upon,
    /// <see cref="ExecuteAsync"/> can be used.
    /// </summary>
    /// <typeparam name="T">The type of this command's parameter</typeparam>
    public class AsyncRelayCommand<T> : AbstractRelayCommand<T>
    {
        private readonly Func<T, Task> _execute;

        /// <param name="execute">The async action that is called when this command is executed.</param>
        /// <param name="canExecute">A predicate returning whether this command can be executed with a given parameter.
        /// Null if all parameters that are of correct type or <c>default(T)</c> can be executed.</param>
        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T> canExecute = null) : base(canExecute)
        {
            _execute = execute;
        }

        protected override async void Execute(T parameter)
        {
            await _execute(parameter);
        }

        /// <summary>
        /// Executes this command asynchronously and returns a task that finishes when the command is finished.
        /// </summary>
        public async Task ExecuteAsync(T parameter)
        {
            await _execute(parameter);
        }
    }
}
