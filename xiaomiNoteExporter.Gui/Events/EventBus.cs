using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace xiaomiNoteExporter.Gui.Events;

/// <summary>
/// Global event bus compatible with WPF threading.
/// </summary>
public static class EventBus
{
    private static readonly ConcurrentDictionary<Type, MulticastDelegate> _handlers = new();

    /// <summary>
    /// Subscribe to an event by adding a handler.
    /// </summary>
    public static void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : Event
    {
        _handlers.AddOrUpdate(
            typeof(TEvent),
            handler,
            (_, existingHandler) => (MulticastDelegate)Delegate.Combine(existingHandler, handler)
        );
    }

    /// <summary>
    /// Unsubscribe from an event by passing the handler to remove.
    /// </summary>
    public static void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : Event
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var existingHandler))
        {
            var newHandler = Delegate.Remove(existingHandler, handler);

            if (newHandler is null)
            {
                _handlers.TryRemove(typeof(TEvent), out _);
            } 
            else
            {
                _handlers[typeof(TEvent)] = (MulticastDelegate)newHandler;
            }
        }
    }

    /// <summary>
    /// Raise the subscribed event by passing the event instance.
    /// </summary>
    public static async Task Raise<TEvent>(TEvent @event) where TEvent : Event
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var handler))
        {
            var dispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (dispatcher is null)
            {
                return;
            }

            foreach (var @delegate in handler.GetInvocationList().Cast<Func<TEvent, Task>>())
            {
                if (dispatcher.CheckAccess())
                {
                    await @delegate(@event);
                } 
                else
                {
                    await dispatcher.InvokeAsync(async () => await @delegate(@event), DispatcherPriority.Normal);
                }
            }
        }
    }
}
