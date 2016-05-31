using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace POESKillTree.Utils.Extensions
{
    public static class Helpers
    {
        public static T FindAnchestor<T>(this DependencyObject current)
            where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        public static T FindParent<T>(this FrameworkElement current)
            where T : FrameworkElement
        {
            do
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = current.Parent as FrameworkElement;
            }
            while (current != null);
            return null;
        }

        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj, DependencyObject stopAt = null) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child == stopAt)
                    continue;

                if (child is T)
                {
                    yield return (T)child;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child, stopAt))
                {
                    yield return childOfChild;
                }
            }
        }
    }
}
