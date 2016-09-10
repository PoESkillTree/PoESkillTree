using System;
using POESKillTree.Utils;

namespace POESKillTree.Controls.Dialogs
{
    public class FileSelectorDialogSettings
    {
        /// <summary>
        /// Gets or sets the path that is initially selected. Defaults to <see cref="AppData.ProgramDirectory"/>.
        /// </summary>
        public string DefaultPath { get; set; } = AppData.ProgramDirectory;

        /// <summary>
        /// Gets or sets whether the path is intepreted as a directory and not a file. Default is false.
        /// </summary>
        public bool IsFolderPicker { get; set; }

        /// <summary>
        /// Gets or sets a relative sub path of the selected path that is required to be creatable.
        /// Only allowed to be non-null if <see name="IsFolderPicker"/> is true. Default is null.
        /// </summary>
        public string ValidationSubPath { get; set; }

        /// <summary>
        /// Gets or sets whether the user can cancel the dialog. Default is true.
        /// </summary>
        public bool IsCancelable { get; set; } = true;

        /// <summary>
        /// Gets or sets a function that validates the path it is given. The function returns a error message if
        /// the path can't be selected that will be displayed to the user. It returns null if the path is valid.
        /// Default is a function that allows all paths.
        /// </summary>
        public Func<string, string> AdditionalValidationFunc { get; set; } = _ => null;
    }
}