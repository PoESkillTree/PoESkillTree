using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace POESKillTree.Utils
{
    /// <summary>
    /// ValidationRule that is valid if the value is not null.
    /// </summary>
    public class NotNullValidationRule : ValidationRule
    {
        /// <summary>
        /// Message to describing the invalid case.
        /// </summary>
        public string Message { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value == null
                ? new ValidationResult(false, Message)
                : ValidationResult.ValidResult;
        }
    }

    /// <summary>
    /// ValidationRule that is valid if the value is no duplicate.
    /// </summary>
    public class DuplicateValidationRule : ValidationRule
    {
        /// <summary>
        /// Message describing the invalid case.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Enumerable to check duplication against.
        /// </summary>
        public IEnumerable ValidationSet { get; set; }

        /// <summary>
        /// The unchanged object in the ValidationSet.
        /// It is valid if the value in Validate() is equal to this one.
        /// </summary>
        public object SelectedValue { get; set; }

        /// <summary>
        /// Selects the value of objects in ValidationSet on which duplication checks are based on.
        /// </summary>
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