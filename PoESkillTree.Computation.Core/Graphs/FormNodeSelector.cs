using System;
using System.Linq;
using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    /// <summary>
    /// Selects a form node collection in an <see cref="IReadOnlyStatGraph"/>/<see cref="IStatGraph"/> using
    /// a <see cref="Form"/> and <see cref="PathDefinition"/>.
    /// </summary>
    public class FormNodeSelector
    {
        private static readonly Form[] MainPathOnlyForms = { Form.TotalOverride };

        public FormNodeSelector(Form form, PathDefinition path)
        {
            if (!path.IsMainPath && MainPathOnlyForms.Contains(form))
                throw new ArgumentException($"{form} is only allowed with the main path");

            Form = form;
            Path = path;
        }

        public Form Form { get; }
        public PathDefinition Path { get; }

        public override bool Equals(object obj) =>
            (obj == this) || (obj is FormNodeSelector other && Equals(other));

        private bool Equals(FormNodeSelector other) =>
            Form.Equals(other.Form) && Path.Equals(other.Path);

        public override int GetHashCode() =>
            (Form, Path).GetHashCode();

        public void Deconstruct(out Form form, out PathDefinition path) =>
            (form, path) = (Form, Path);
    }
}