using System;

namespace xiaomiNoteExporter.Gui.Events;

/// <summary>
/// Custom <c>Event</c> base class.
/// </summary>
public abstract class Event
{
    /// <summary>
    /// Timestamp indicating raised time.
    /// </summary>
    public DateTime Timestamp { get; } = DateTime.UtcNow; 
}
