namespace PoESkillTree.Computation.Console.Builders
{
    /// <summary>
    /// Base class for builder implementations consisting of a string that is returned by <see cref="ToString"/> and
    /// is used to implement structural equality.
    /// </summary>
    public abstract class BuilderStub
    {
        private readonly string _stringRepresentation;

        protected BuilderStub(string stringRepresentation)
        {
            _stringRepresentation = stringRepresentation;
        }

        protected BuilderStub(BuilderStub toCopy)
        {
            _stringRepresentation = toCopy._stringRepresentation;
        }

        private bool Equals(BuilderStub other)
        {
            return string.Equals(_stringRepresentation, other._stringRepresentation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BuilderStub) obj);
        }

        public override int GetHashCode()
        {
            return _stringRepresentation.GetHashCode();
        }

        public override string ToString()
        {
            return _stringRepresentation;
        }
    }
}