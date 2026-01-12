namespace xiaomiNoteExporter.Models;

public sealed class NoteDto
{
    public string Id { get; init; } = default!;

    public string Content { get; init; } = default!;

    public DateTime CreationDate { get; init; }

    public DateTime LastModified { get; init; }
}
