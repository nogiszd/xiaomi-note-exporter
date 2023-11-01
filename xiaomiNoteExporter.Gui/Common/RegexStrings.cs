namespace xiaomiNoteExporter.Gui.Common
{
    public sealed class RegexStrings
    {
        public static string Domain => @"^(?!https?:\/\/|ftp:\/\/)([a-zA-Z0-9.-]+)\.([a-zA-Z]{2,})(:[0-9]+)?(\/[^\\s]*)?$";
    }
}
