using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace POESKillTree.Utils
{
    public class NotNullValidationRule : ValidationRule
    {
        public string Message { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value == null
                ? new ValidationResult(false, Message)
                : ValidationResult.ValidResult;
        }
    }

    public class DuplicateValidationRule : ValidationRule
    {
        public string Message { get; set; }

        public IEnumerable ValidationSet { get; set; }

        public object SelectedValue { get; set; }

        public Func<object, object> SelectorFunc { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || ValidationSet == null) return ValidationResult.ValidResult;

            return ValidationSet.Cast<object>().Any(obj => obj != SelectedValue && SelectorFunc(obj) == value)
                ? new ValidationResult(false, Message)
                : ValidationResult.ValidResult;
        }
    }
}