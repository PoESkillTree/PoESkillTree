using System.Threading.Tasks;
using POESKillTree.Controls.Dialogs;
using POESKillTree.Model.Builds;

namespace POESKillTree.Model.Serialization
{
    /// <summary>
    /// Implements all abstract methods of <see cref="AbstractPersistentData"/> by throwing exceptions on call.
    /// Only use this class in tests or if the instance is immediately serialized and not used any further.
    /// </summary>
    public class BarePersistentData : AbstractPersistentData
    {
        public override Task InitializeAsync(IDialogCoordinator dialogCoordinator)
        {
            throw new System.NotSupportedException();
        }

        public override void Save()
        {
            throw new System.NotSupportedException();
        }

        public override void SaveFolders()
        {
            throw new System.NotSupportedException();
        }

        public override void SaveBuild(IBuild build)
        {
            throw new System.NotSupportedException();
        }

        public override void DeleteBuild(IBuild build)
        {
            throw new System.NotSupportedException();
        }

        public override Task ReloadBuildsAsync()
        {
            throw new System.NotSupportedException();
        }

        public override Task<PoEBuild> ImportBuildAsync(string buildXml)
        {
            throw new System.NotSupportedException();
        }

        public override string ExportBuild(PoEBuild build)
        {
            throw new System.NotSupportedException();
        }
    }
}