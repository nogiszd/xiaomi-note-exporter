using System.Xml;
using xiaomiNoteExporter.Gui.Entities;

namespace xiaomiNoteExporter.Gui.Extensions
{
    public static class XmlExtensions
    {
        public static XmlDocument Initialize(string rootName)
        {
            var doc = new XmlDocument();

            XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(xmlDeclaration);

            var root = doc.CreateElement(rootName);

            doc.AppendChild(root);

            return doc;
        }

        public static void AppendNote(this XmlDocument doc, string name, string value, string createdAt, NoteType type)
        {
            var note = doc.CreateElement("note");

            var titleNode = doc.CreateElement("title");
            titleNode.InnerText = name;

            var contentNode = doc.CreateElement("content");
            contentNode.InnerText = value;

            var createdAtNode = doc.CreateElement("createdAt");
            createdAtNode.InnerText = createdAt;

            var typeAttr = doc.CreateAttribute("type");
            typeAttr.Value = type.ToString();

            // append the attribute to the node
            note.Attributes.Append(typeAttr);
            note.AppendChild(titleNode);
            note.AppendChild(contentNode);
            note.AppendChild(createdAtNode);

            doc.DocumentElement?.AppendChild(note);
        }
    }
}
