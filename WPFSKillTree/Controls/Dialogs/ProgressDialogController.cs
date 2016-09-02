using System;
using System.Threading.Tasks;

namespace POESKillTree.Controls.Dialogs
{
    // Interface copied from https://github.com/MahApps/MahApps.Metro/blob/1.2.4/MahApps.Metro/Controls/Dialogs/DialogCoordinator.cs 
    // (licensed under Microsoft Public License as found on https://github.com/MahApps/MahApps.Metro/blob/1.2.4/LICENSE)
    // to solve namespace conflicts with MahApps.Metro.Controls.Dialogs in one place.
    /// <summary>A class for manipulating an open ProgressDialog.</summary>
    public class ProgressDialogController
    {
        private readonly MahApps.Metro.Controls.Dialogs.ProgressDialogController _wrapped;

        public ProgressDialogController(MahApps.Metro.Controls.Dialogs.ProgressDialogController wrapped)
        {
            _wrapped = wrapped;
            _wrapped.Canceled += (sender, args) => Canceled?.Invoke(this, EventArgs.Empty);
            _wrapped.Closed += (sender, args) => Closed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sets the ProgressBar's IsIndeterminate to true. To set it to false, call SetProgress.
        /// </summary>
        public void SetIndeterminate() => _wrapped.SetIndeterminate();

        /// <summary>Sets if the Cancel button is visible.</summary>
        /// <param name="value"></param>
        public void SetCancelable(bool value) => _wrapped.SetCancelable(value);

        /// <summary>
        /// Sets the dialog's progress bar value and sets IsIndeterminate to false.
        /// </summary>
        /// <param name="value">The percentage to set as the value.</param>
        public void SetProgress(double value) => _wrapped.SetProgress(value);

        /// <summary>Sets the dialog's message content.</summary>
        /// <param name="message">The message to be set.</param>
        public void SetMessage(string message) => _wrapped.SetMessage(message);

        /// <summary>Sets the dialog's title.</summary>
        /// <param name="title">The title to be set.</param>
        public void SetTitle(string title) => _wrapped.SetTitle(title);

        /// <summary>Begins an operation to close the ProgressDialog.</summary>
        /// <returns>A task representing the operation.</returns>
        public Task CloseAsync() => _wrapped.CloseAsync();

        /// <summary>Gets if the Cancel button has been pressed.</summary>
        public bool IsCanceled => _wrapped.IsCanceled;

        /// <summary>Gets if the wrapped ProgressDialog is open.</summary>
        public bool IsOpen => _wrapped.IsOpen;

        /// <summary>
        ///  Gets/Sets the minimum restriction of the progress Value property
        /// </summary>
        public double Minimum
        {
            get { return _wrapped.Minimum; }
            set { _wrapped.Minimum = value; }
        }

        /// <summary>
        ///  Gets/Sets the maximum restriction of the progress Value property
        /// </summary>
        public double Maximum
        {
            get { return _wrapped.Maximum; }
            set { _wrapped.Maximum = value; }
        }

        /// <summary>
        /// This event is raised when the associated <see cref="T:MahApps.Metro.Controls.Dialogs.ProgressDialog" /> was closed programmatically.
        /// </summary>
        public event EventHandler Closed;
        /// <summary>
        /// This event is raised when the associated <see cref="T:MahApps.Metro.Controls.Dialogs.ProgressDialog" /> was cancelled by the user.
        /// </summary>
        public event EventHandler Canceled;
    }
}