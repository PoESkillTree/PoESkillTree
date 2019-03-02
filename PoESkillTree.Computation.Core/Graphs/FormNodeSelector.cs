using System;
using PoESkillTree.Computation.Common;
using PoESkillTree.Utils;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Selects a form node collection in an <see cref="IReadOnlyStatGraph"/>/<see cref="IStatGraph"/> using
    /// a <see cref="Form"/> and <see cref="PathDefinition"/>.
    /// </summary>
    public class FormNodeSelector : ValueObject
    {
        private const Form MainPathOnlyForm = Form.TotalOverride;

        public FormNodeSelector(Form form, PathDefinition path)
            : base(true)
        {
            if (form == MainPathOnlyForm && !path.IsMainPath)
                throw new ArgumentException($"{form} is only allowed with the main path");

            Form = form;
            Path = path;
        }

        public Form Form { get; }
        public PathDefinition Path { get; }

        protected override object ToTuple() => (Form, Path);
    }
}