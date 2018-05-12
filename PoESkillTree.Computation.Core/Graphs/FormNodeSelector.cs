using PoESkillTree.Computation.Common;

namespace PoESkillTree.Computation.Core.Graphs
{
    public class FormNodeSelector
    {
        public FormNodeSelector(Form form, PathDefinition path)
        {
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