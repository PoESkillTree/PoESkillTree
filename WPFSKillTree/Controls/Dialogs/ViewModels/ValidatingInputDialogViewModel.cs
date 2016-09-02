using System;
using System.Collections.Generic;
using POESKillTree.Common.ViewModels;

namespace POESKillTree.Controls.Dialogs.ViewModels
{
    /// <summary>
    /// View model for a input dialog that validates the input.
    /// </summary>
    public class ValidatingInputDialogViewModel : ErrorInfoViewModel<string>
    {
        private readonly Func<string, string> _inputValidationFunc;
        private string _input;

        public string Message { get; }

        public string Input
        {
            get { return _input; }
            set { SetProperty(ref _input, value); }
        }

        public ValidatingInputDialogViewModel(string title, string message, string defaultText, Func<string, string> inputValidationFunc)
        {
            _inputValidationFunc = inputValidationFunc;
            DisplayName = title;
            Message = message;
            Input = defaultText;
        }
        
        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            return propertyName != nameof(Input) ? null : new[] {_inputValidationFunc(Input)};
        }
    }
}