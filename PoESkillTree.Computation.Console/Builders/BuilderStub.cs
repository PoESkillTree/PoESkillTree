namespace PoESkillTree.Computation.Console.Builders
{
    public abstract class BuilderStub
    {
        private readonly string _stringRepresentation;

        protected BuilderStub(string stringRepresentation)
        {
            _stringRepresentation = stringRepresentation;
        }

        public override string ToString()
        {
            return _stringRepresentation;
        }
    }
}