using System;

namespace xiaomiNoteExporter.Gui.Events;

public abstract class Event
{
    public DateTime Timestamp { get; } = DateTime.UtcNow; 
}
