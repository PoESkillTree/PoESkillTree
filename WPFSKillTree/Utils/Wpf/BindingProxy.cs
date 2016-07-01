using System.Windows;

namespace POESKillTree.Utils.Wpf
{
    /// <summary>
    /// Wraps any object of type T to allow binding to it. Use this if the original object is not
    /// accessible in your context.
    /// <para>
    /// To be used from Xaml, you need to create a non-generic subclass:
    /// <code>public class YourViewModelBindingProxy : BindingProxy&lt;YourViewModel&gt; { }</code>
    /// </para>
    /// <para>
    /// One use case is accessing your control's ViewModel in a ContextMenu that has a different DataContext
    /// (e.g. if it's in a Template), as ContextMenus lie in a different visual tree.
    /// </para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindingProxy<T> : Freezable
    {
        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy<T>();
        }

        public T Data
        {
            get { return (T) GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(T), typeof(BindingProxy<T>), new UIPropertyMetadata(null));
    }
}