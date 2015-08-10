using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;

namespace POESKillTree.TreeGenerator.Views
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

        public ObservableCollection<string> ValidationSet { get; set; }

        // TODO find a way to bind AdvancedTabViewModel.SelectedAttributeConstraint.Attribute or anything else here

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return value == null
                ? new ValidationResult(false, Message)
                : ValidationResult.ValidResult;
        }
    }
}