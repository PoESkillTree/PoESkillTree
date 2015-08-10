using System;
using System.ComponentModel;
using System.Diagnostics;

namespace POESKillTree.Utils
{
    public static class EventHandlerExtensions
    {
        public static void Raise(this PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            VerifyPropertyName(sender, propertyName);
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        // Source: https://msdn.microsoft.com/en-us/magazine/dd419663.aspx
        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This 
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private static void VerifyPropertyName(object sender, string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(sender)[propertyName] != null) return;

            var msg = "Invalid property name: " + propertyName;

            throw new Exception(msg);
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