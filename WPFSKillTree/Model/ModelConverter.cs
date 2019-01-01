using PoESkillTree.GameModel.PassiveTree;
using POESKillTree.SkillTreeFiles;

namespace POESKillTree.Model
{
    /// <summary>
    /// The PoESkillTree.GameModel project should at some point replace all game-related model classes.
    /// Because the Computation projects already use the new model, this class has to exist to convert between
    /// the old and new model.
    /// </summary>
    public static class ModelConverter
    {
        public static PassiveNodeDefinition Convert(SkillNode skillNode)
            => new PassiveNodeDefinition(
                skillNode.Id,
                skillNode.Type,
                skillNode.Name,
                skillNode.ascendancyName != null,
                skillNode.passivePointsGranted,
                skillNode.attributes);
    }
}