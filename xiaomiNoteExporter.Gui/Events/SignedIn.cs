using System;

namespace xiaomiNoteExporter.Gui.Events
{
    public class SignedInEvent
    {
        public delegate void SignedInHandler(object sender, EventArgs e);

        public event SignedInHandler? SignedIn;

        protected virtual void OnSignedIn()
        {
            SignedInHandler? handler = SignedIn;

            if (handler != null)
            {
                EventArgs e = new();
                handler(this, e);
            }
        }

        public void Raise()
        {
            OnSignedIn();
        }
    }
}
