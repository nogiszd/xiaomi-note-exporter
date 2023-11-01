using System.Globalization;
using System.Windows.Controls;

namespace xiaomiNoteExporter.Gui.Validation
{
    public class DomainValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string text = value.ToString()!;

            if (string.IsNullOrEmpty(text))
            {
                return new(false, "Domain should not be empty");
            }

            return ValidationResult.ValidResult;
        }
    }
}
