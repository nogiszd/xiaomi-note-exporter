using System.Xml;

using xiaomiNoteExporter.Gui.Entities;

namespace xiaomiNoteExporter.Gui.Extensions;

public static class XmlExtensions
{
    /// <summary>
    /// Initialize XML document header and root element.
    /// </summary>
    /// <param name="rootName">Name of the root node.</param>
    /// <returns>New preinitialized <c>XmlDocument</c> instance.</returns>
    public static XmlDocument Initialize(string rootName)
    {
        var doc = new XmlDocument();

        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        doc.AppendChild(xmlDeclaration);

        var root = doc.CreateElement(rootName);

        doc.AppendChild(root);

        return doc;
    }

    /// <summary>
    /// Append new note node to the XML document.
    /// </summary>
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

        note.Attributes.Append(typeAttr);
        note.AppendChild(titleNode);
        note.AppendChild(contentNode);
        note.AppendChild(createdAtNode);

        doc.DocumentElement?.AppendChild(note);
    }
}
