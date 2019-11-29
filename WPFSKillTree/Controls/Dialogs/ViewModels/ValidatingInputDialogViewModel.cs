using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Common.ViewModels;

namespace PoESkillTree.Controls.Dialogs.ViewModels
{
    /// <summary>
    /// View model for a input dialog that validates the input.
    /// </summary>
    public class ValidatingInputDialogViewModel : ErrorInfoViewModel<string?>
    {
        private readonly Func<string, string?> _inputValidationFunc;
        private string _input;

        public string Message { get; }

        public string Input
        {
            get { return _input; }
            set { SetProperty(ref _input, value); }
        }

#pragma warning disable CS8618 // _input is set through Input
        public ValidatingInputDialogViewModel(string title, string message, string defaultText, Func<string, string?> inputValidationFunc)
#pragma warning restore
        {
            _inputValidationFunc = inputValidationFunc;
            DisplayName = title;
            Message = message;
            Input = defaultText;
        }
        
        protected override IEnumerable<string?> ValidateProperty(string propertyName)
        {
            return propertyName != nameof(Input) ? Enumerable.Empty<string?>() : new[] {_inputValidationFunc(Input)};
        }
    }
}