using System;
using System.Collections.Generic;
using System.Windows;

namespace POESKillTree.Controls.Dialogs
{
    public delegate void ContextRegistrationChangedEventHandler(object context, DependencyObject association);

    // This is a copy of https://github.com/MahApps/MahApps.Metro/blob/1.2.4/MahApps.Metro/Controls/Dialogs/DialogParticipation.cs
    // (licensed under Microsoft Public License as found on https://github.com/MahApps/MahApps.Metro/blob/1.2.4/LICENSE)
    // to be able to access its internal methods and to add a changed event.
    public static class DialogParticipation
    {
        private static readonly IDictionary<object, DependencyObject> ContextRegistrationIndex = new Dictionary<object, DependencyObject>();

        public static readonly DependencyProperty RegisterProperty = DependencyProperty.RegisterAttached(
            "Register", typeof(object), typeof(DialogParticipation), new PropertyMetadata(default(object), RegisterPropertyChangedCallback));

        private static void RegisterPropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (dependencyPropertyChangedEventArgs.OldValue != null)
                ContextRegistrationIndex.Remove(dependencyPropertyChangedEventArgs.OldValue);

            if (dependencyPropertyChangedEventArgs.NewValue != null)
                ContextRegistrationIndex[dependencyPropertyChangedEventArgs.NewValue] = dependencyObject;

            if (ContextRegistrationChanged != null)
                ContextRegistrationChanged(dependencyPropertyChangedEventArgs.NewValue, dependencyObject);
        }

        public static void SetRegister(DependencyObject element, object context)
        {
            element.SetValue(RegisterProperty, context);
        }

        public static object GetRegister(DependencyObject element)
        {
            return element.GetValue(RegisterProperty);
        }

        internal static bool IsRegistered(object context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return ContextRegistrationIndex.ContainsKey(context);
        }

        internal static DependencyObject GetAssociation(object context)
        {
            if (context == null) throw new ArgumentNullException("context");

            return ContextRegistrationIndex[context];
        }

        public static event ContextRegistrationChangedEventHandler ContextRegistrationChanged;
    }
}