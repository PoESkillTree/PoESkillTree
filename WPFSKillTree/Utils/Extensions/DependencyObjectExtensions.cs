using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using MahApps.Metro.Controls;

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

        /// <summary>
        /// Returns the MetroWindow this dependency object is located in.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this dependency object is not located in a MetroWindow
        /// </exception>
        public static MetroWindow GetMetroWindow(this DependencyObject dependencyObject)
        {
            var window = Window.GetWindow(dependencyObject) as MetroWindow;
            if (window == null)
                throw new InvalidOperationException("This dependency object is not located in a MetroWindow");
            return window;
        }
    }
}