using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PoESkillTree.Common.ViewModels
{
    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates.
    /// </summary>
    public class RelayCommand : AbstractRelayCommand
    {
        private readonly Action _execute;

        /// <param name="execute">The action that is called when this command is executed.</param>
        /// <param name="canExecute">A function returning whether this command can currently be executed.
        /// Null if all parameters can be executed.</param>
        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : base(_ => canExecute is null || canExecute())
        {
            _execute = execute;
        }

        public override void Execute(object? parameter)
        {
            _execute();
        }
    }

    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates. The execution delegate is async, however, this is
    /// mostly for convenience as users of <see cref="ICommand"/>s can't wait on async execution
    /// (<see cref="ICommand.Execute"/> will return at the first await call).
    /// </summary>
    public class AsyncRelayCommand : AbstractRelayCommand
    {
        private readonly Func<Task> _execute;

        /// <param name="execute">The async action that is called when this command is executed.</param>
        /// <param name="canExecute">A function returning whether this command can currently be executed.
        /// Null if all parameters can be executed.</param>
        public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
            : base(_ => canExecute is null || canExecute())
        {
            _execute = execute;
        }

        public override async void Execute(object? parameter)
        {
            await _execute();
        }
    }

    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates.
    /// </summary>
    /// <typeparam name="T">The type of this command's parameter</typeparam>
    public class RelayCommand<T> : AbstractRelayCommand
    {
        private readonly Action<T> _execute;

        /// <param name="execute">The action that is called when this command is executed.</param>
        /// <param name="canExecute">A predicate returning whether this command can be executed with a given parameter.
        /// Null if all parameters that are of correct type or <c>default(T)</c> can be executed.</param>
        /// <param name="allowNull">If true, null parameters can be executed. Only set this to true if T is nullable.</param>
        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null, bool allowNull = false)
            : base(TypeSafeCanExecute(canExecute, allowNull))
        {
            _execute = execute;
        }

        public override void Execute(object? parameter)
        {
            _execute((T) parameter!);
        }
    }

    /// <summary>
    /// <see cref="ICommand"/> implementation using delegates. The execution delegate is async, however, this is
    /// mostly for convenience as users of <see cref="ICommand"/>s can't wait on async execution
    /// (<see cref="ICommand.Execute"/> will return at the first await call).
    /// </summary>
    /// <typeparam name="T">The type of this command's parameter</typeparam>
    public class AsyncRelayCommand<T> : AbstractRelayCommand
    {
        private readonly Func<T, Task> _execute;

        /// <param name="execute">The async action that is called when this command is executed.</param>
        /// <param name="canExecute">A predicate returning whether this command can be executed with a given parameter.
        /// Null if all parameters that are of correct type or <c>default(T)</c> can be executed.</param>
        public AsyncRelayCommand(Func<T, Task> execute, Predicate<T>? canExecute = null)
            : base(TypeSafeCanExecute(canExecute, false))
        {
            _execute = execute;
        }

        public override async void Execute(object? parameter)
        {
            await _execute((T) parameter!);
        }
    }

    /// <summary>
    /// Abstract <see cref="ICommand"/> implementation. Only leaves the execution for subclasses to implement.
    /// </summary>
    public abstract class AbstractRelayCommand : ICommand
    {
        private readonly Predicate<object?> _canExecute;

        protected AbstractRelayCommand(Predicate<object?>? canExecute)
        {
            _canExecute = canExecute ?? (_ => true);
        }

        [DebuggerStepThrough]
        public bool CanExecute(object? parameter)
            => _canExecute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        protected static Predicate<object?> TypeSafeCanExecute<T>(Predicate<T>? canExecute, bool allowNull) =>
            p => p switch
            {
                T t => (canExecute is null || canExecute(t)),
                null => (canExecute is null || allowNull),
                _ => false
            };

        public abstract void Execute(object? parameter);
    }
}
