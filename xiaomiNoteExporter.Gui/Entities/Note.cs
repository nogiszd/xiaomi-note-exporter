namespace xiaomiNoteExporter.Gui.Entities
{
    public sealed class Note
    {
        public string? Title { get; private set; }

        public string? Content { get; private set; }

        public string CreatedAt { get; private set; } = string.Empty;

        public NoteType Type { get; private set; }

        public static Note Create(string? title, string? content, string createdAt, NoteType type)
        {
            var note = new Note
            {
                Title = title,
                Content = content,
                CreatedAt = createdAt,
                Type = type
            };

            return note;
        }
    }

    public enum NoteType
    {
        Normal,
        Unsupported,
    }
}
