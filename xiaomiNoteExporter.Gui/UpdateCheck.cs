using HtmlAgilityPack;
using SystemVersion = System.Version;
using System.Net.Http;
using System.Threading.Tasks;
using xiaomiNoteExporter.Gui.Common;

namespace xiaomiNoteExporter.Gui
{
    public sealed class UpdateCheck
    {
        private readonly string _xpath = @"//*[name()='svg' and contains(@class, 'tag')]/following-sibling::span";

        public async Task<(string, bool)> Check()
        {
            using HttpClient client = new();
            using HttpResponseMessage response = await client.GetAsync(Properties.Settings.Default.repo);
            using HttpContent content = response.Content;

            string result = await content.ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(result);

            return DoesNewerExist(GetVersion(document));
        }

        private string GetVersion(HtmlDocument document)
        {
            string version = document.DocumentNode.SelectSingleNode(_xpath).InnerText.Trim();

            return version.Replace("v", string.Empty);
        }

        private (string, bool) DoesNewerExist(string version)
        {
            bool flag = false;
            Version currentVersion = new();

            var v = new SystemVersion(version);
                
            if (v.CompareTo(currentVersion.Current) > 0) 
                flag = true;

            return (version, flag);
        }
    }
}
