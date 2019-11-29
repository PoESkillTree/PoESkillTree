using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Common.ViewModels
{
    /// <summary>
    /// Base class for closeable view models that implements <see cref="INotifyDataErrorInfo"/>.
    /// Abstracts from <see cref="INotifyDataErrorInfo"/> and provides methods that allow setting and removing
    /// errors for individual properties.
    /// Uses error information to determine whether the view model can currently be closed.
    /// </summary>
    /// <remarks>
    /// Subclasses need to implement <see cref="ValidateProperty"/>. It is called for a property when it is changed.
    /// If property changes affect the errors of other properties, <see cref="SetErrors"/>
    /// or <see cref="ClearErrors(string)"/> can be used.
    /// </remarks>
    /// <typeparam name="T">Type of the parameter used for the close command.</typeparam>
    public abstract class ErrorInfoViewModel<T> : CloseableViewModel<T>, INotifyDataErrorInfo
    {
        private readonly IDictionary<string, IList<string>> _errorDict = new Dictionary<string, IList<string>>();

        public bool HasErrors
        {
            get { return _errorDict.Values.Any(l => l.Any()); }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return _errorDict.Values.Flatten();
            }

            return _errorDict.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
        }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        /// <summary>
        /// Returns all error messages of the given property. Is called after the property is changed
        /// (but before changed handlers are called).
        /// </summary>
        /// <returns>All current error messages of the given property. Null or an empty enumerable means that
        /// there are no errors.</returns>
        protected abstract IEnumerable<string?> ValidateProperty(string propertyName);

        /// <summary>
        /// Sets the errors for the given property.
        /// If <paramref name="errors"/> is null or empty, the errors of <paramref name="propertyName"/> are cleared.
        /// If an entry of <paramref name="errors"/> is null or empty, it is ignored.
        /// </summary>
        private void SetErrors(string propertyName, IEnumerable<string?> errors)
        {
            var errorList = errors.WhereNotNull().Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (!errorList.Any())
            {
                ClearErrors(propertyName);
                return;
            }

            if (_errorDict.TryGetValue(propertyName, out var oldList) && oldList.Count == errorList.Count &&
                oldList.SequenceEqual(errorList))
                return;
            _errorDict[propertyName] = errorList;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Removes the errors for the given property.
        /// </summary>
        private void ClearErrors(string propertyName)
        {
            if (_errorDict.TryGetValue(propertyName, out var list) && list.Any())
            {
                list.Clear();
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected override bool CanClose(T param)
            => !HasErrors;

        protected override void OnPropertyChanged(string propertyName)
        {
            SetErrors(propertyName, ValidateProperty(propertyName));
            base.OnPropertyChanged(propertyName);
        }
    }
}