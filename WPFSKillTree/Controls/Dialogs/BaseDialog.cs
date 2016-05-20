using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;

namespace POESKillTree.Controls.Dialogs
{
    /// <summary>
    /// Subclass of <see cref="BaseMetroDialog"/> that has a few additional bindings to adjust
    /// the appearance.
    /// <para/>
    /// If you want to make the close button visible and have a view model that is a subclass of
    /// <see cref="ViewModels.ViewModelBase"/>, use <see cref="CloseableBaseDialog"/> instead.
    /// </summary>
    public class BaseDialog : BaseMetroDialog
    {
        public static readonly DependencyProperty MaxContentWidthProperty =
            DependencyProperty.Register("MaxContentWidth", typeof(double), typeof(BaseDialog), new PropertyMetadata(double.PositiveInfinity));

        /// <summary>
        /// Gets or sets the maximum width of the contents of this dialog.
        /// The width normally is 50% of the parent window, that can be limited with this.
        /// <para/>
        /// Default: <see cref="double.PositiveInfinity"/>
        /// </summary>
        public double MaxContentWidth
        {
            get { return (double) GetValue(MaxContentWidthProperty); }
            set { SetValue(MaxContentWidthProperty, value); }
        }

        public static readonly DependencyProperty CloseButtonVisibilityProperty =
            DependencyProperty.Register("CloseButtonVisibility", typeof(Visibility), typeof(BaseDialog), new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets or sets the visibility level of the part of this dialog which shows the close button.
        /// <para/>
        /// Default: <see cref="Visibility.Collapsed"/>
        /// </summary>
        public Visibility CloseButtonVisibility
        {
            get { return (Visibility) GetValue(CloseButtonVisibilityProperty); }
            set { SetValue(CloseButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(BaseDialog), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the command that is executed on close button activation.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return (ICommand) GetValue(CloseCommandProperty); }
            set { SetValue(CloseCommandProperty, value); }
        }

        static BaseDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BaseDialog),
                         new FrameworkPropertyMetadata(typeof(BaseDialog)));
        }
    }

    /// <summary>
    /// Subclass of <see cref="BaseDialog"/> that uses a DataContext of type <see cref="ViewModels.CloseableViewModel"/>
    /// to set <see cref="BaseMetroDialog.Title"/> to DisplayName of the view model and <see cref="BaseDialog.CloseCommand"/>
    /// to CloseCommand of the view model. The close button is visible by default.
    /// </summary>
    public class CloseableBaseDialog : BaseDialog
    {
        static CloseableBaseDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableBaseDialog),
                         new FrameworkPropertyMetadata(typeof(CloseableBaseDialog)));
        }
    }
}