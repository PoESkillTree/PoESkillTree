using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace POESKillTree.Utils
{
    public static class EventHandlerExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, [CallerMemberName] string propertyName = null)
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