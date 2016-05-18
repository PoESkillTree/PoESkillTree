using System;
using System.ComponentModel;

namespace POESKillTree.Utils
{
    /// <summary>
    /// Provides extension methods for EventHandlers.
    /// </summary>
    public static class EventHandlerExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static void Raise<T>(this EventHandler<T> handler, object sender, T eventArgs)
        {
            if (handler != null)
            {
                handler(sender, eventArgs);
            }
        }

        public static void Raise(this EventHandler handler, object sender)
        {
            if (handler != null)
            {
                handler(sender, EventArgs.Empty);
            }
        }
    }
}