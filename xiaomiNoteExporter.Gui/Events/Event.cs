using System;
using System.Threading.Tasks;

namespace xiaomiNoteExporter.Gui.Events
{
    public abstract class Event
    {
        public delegate void EventHandler(object sender, EventArgs e);

        public event EventHandler? Handler;

        protected virtual void OnEvent()
        {
            EventHandler? handler = Handler;

            if (handler != null)
            {
                EventArgs e = new();
                handler(this, e);
            }
        }

        public async Task Raise()
        {
            await Task.Run(OnEvent);
        }
    }
}
