using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using xiaomiNoteExporter.Gui.Common;

namespace xiaomiNoteExporter.Gui.Validators;

public class DomainValidationRule : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
        string text = value.ToString()!;

        var regexResult = new Regex(RegexStrings.Domain).Match(text);

        if (string.IsNullOrEmpty(text))
        {
            return new(false, "Domain should not be empty");
        }

        if (!regexResult.Success)
        {
            return new(false, "Domain is not valid");
        }

        return ValidationResult.ValidResult;
    }
}
