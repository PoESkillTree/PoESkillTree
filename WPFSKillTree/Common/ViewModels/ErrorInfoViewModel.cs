using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using POESKillTree.Utils.Extensions;

namespace POESKillTree.Common.ViewModels
{
    public abstract class ErrorInfoViewModel<T> : CloseableViewModel<T>, INotifyDataErrorInfo
    {
        private readonly IDictionary<string, IList<string>> _errorDict = new Dictionary<string, IList<string>>();

        public bool HasErrors
        {
            get { return _errorDict.Values.Any(l => l.Any()); }
        }

        protected bool AlwaysAllowNullParamClose { get; set; } = true;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return _errorDict.Values.Flatten();
            }
            IList<string> list;
            if (_errorDict.TryGetValue(propertyName, out list))
            {
                return list;
            }
            return null;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        protected abstract IEnumerable<string> ValidateProperty(string propertyName);

        protected void SetError(string propertyName, string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ClearErrors(propertyName);
                return;
            }
            IList<string> oldList;
            if (_errorDict.TryGetValue(propertyName, out oldList) && oldList.Count == 1 && oldList[0] == error)
                return;
            _errorDict[propertyName] = new[] {error};
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void SetErrors(string propertyName, IEnumerable<string> errors)
        {
            if (errors == null)
            {
                ClearErrors(propertyName);
                return;
            }
            var errorList = errors.Where(s => !string.IsNullOrEmpty(s)).ToList();
            if (!errorList.Any())
            {
                ClearErrors(propertyName);
                return;
            }
            IList<string> oldList;
            if (_errorDict.TryGetValue(propertyName, out oldList) && oldList.Count == errorList.Count &&
                oldList.SequenceEqual(errorList))
                return;
            _errorDict[propertyName] = errorList;
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        protected void ClearErrors(string propertyName)
        {
            IList<string> list;
            if (!_errorDict.TryGetValue(propertyName, out list))
                return;
            if (list.Any())
            {
                list.Clear();
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        protected void ClearErrors()
        {
            _errorDict.Clear();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(null));
        }

        protected override bool CanClose(T param)
        {
            return !HasErrors || (AlwaysAllowNullParamClose && param == null);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            SetErrors(propertyName, ValidateProperty(propertyName));
            base.OnPropertyChanged(propertyName);
        }
    }
}