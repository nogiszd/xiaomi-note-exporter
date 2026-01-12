using Pastel;
using System.Drawing;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using xiaomiNoteExporter.Models;

namespace xiaomiNoteExporter;

public class JsonConverter(string path)
{
    private readonly string _path = path ?? throw new ArgumentNullException(nameof(path));
    private static readonly string baseDir = AppDomain.CurrentDomain.BaseDirectory;

    public void Start()
    {
        IReadOnlyList<NoteDto> notes;
        string outputPath;

        if (Directory.Exists(_path))
        {
            Console.WriteLine($"\rParsing from directory...");
            notes = ConvertFromDirectory(_path);

            var dirName = new DirectoryInfo(_path).Name;

            outputPath = Path.Combine(baseDir, $"{dirName}.json");
        }
        else if (File.Exists(_path))
        {
            Console.WriteLine($"\rParsing from file...");
            notes = ConvertFromFile(_path);

            var fileName = Path.GetFileNameWithoutExtension(_path);
            outputPath = Path.Combine(baseDir, $"{fileName}.json");
        }
        else
        {
            throw new FileNotFoundException("Provided path doesn't exist.", _path);
        }

        var json = JsonSerializer.Serialize(notes, NoteDtoContext.Default.NoteDtoArray);

        File.WriteAllText(outputPath, json, Encoding.UTF8);

        Console.Clear();
        Console.WriteLine($"Successfully converted notes to {outputPath.Pastel(Color.WhiteSmoke)}\n".Pastel(Color.LimeGreen));
        Console.WriteLine("Press any key to close application...".Pastel(Color.Gray));
        Console.ReadKey();
    }

    private static List<NoteDto> ConvertFromDirectory(string dir)
    {
        var notes = new List<NoteDto>();

        foreach (var file in Directory.GetFiles(dir, "*.md"))
        {
            var content = File.ReadAllText(file);
            notes.Add(ParseSingle(content));
        }

        return notes;
    }

    private static IReadOnlyList<NoteDto> ConvertFromFile(string path)
    {
        var content = File.ReadAllText(path);

        if (content.Contains("****"))
        {
            return content
                .Split("****", StringSplitOptions.RemoveEmptyEntries)
                .Select(ParseSingle)
                .ToList();
        }

        return new[] { ParseSingle(content) };
    }

    private static NoteDto ParseSingle(string raw)
    {
        var lines = raw
            .Replace("\r\n", "\n")
            .Split('\n')
            .Select(l => l.TrimEnd())
            .ToList();

        var createdLine = lines.LastOrDefault(l => l.StartsWith("*Created at:")) ?? throw new FormatException("Invalid note format.");

        var dateText = createdLine
            .Replace("*Created at:", string.Empty)
            .Replace("*", string.Empty)
            .Trim();

        var createdDate = DateTime.ParseExact(dateText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        lines.RemoveAt(lines.LastIndexOf(createdLine));

        if (lines.Count > 0 && lines[0].StartsWith("## "))
        {
            lines.RemoveAt(0);
        }

        var content = string.Join("\n", lines).Trim();

        return new NoteDto
        {
            Id = ComputeMd5(content),
            Content = content,
            CreationDate = createdDate,
            LastModified = createdDate,
        };
    }

    private static string ComputeMd5(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
[JsonSerializable(typeof(NoteDto[]))]
internal partial class NoteDtoContext : JsonSerializerContext { }