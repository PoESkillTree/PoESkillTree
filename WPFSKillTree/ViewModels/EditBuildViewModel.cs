using POESKillTree.Model.Builds;

namespace POESKillTree.ViewModels
{
    public class EditBuildViewModel : CloseableViewModel<bool>
    {
        public PoEBuild Build { get; }

        public EditBuildViewModel(PoEBuild build)
        {
            Build = build.DeepClone();
        }
    }
}