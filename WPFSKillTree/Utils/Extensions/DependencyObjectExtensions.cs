using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace POESKillTree.Utils.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) yield break;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                {
                    yield return (T)child;
                }

                foreach (var childOfChild in child.FindVisualChildren<T>())
                {
                    yield return childOfChild;
                }
            }
        }
    }
}