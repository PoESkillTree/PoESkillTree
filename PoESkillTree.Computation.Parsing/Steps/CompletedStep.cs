using System.Collections.Generic;

namespace PoESkillTree.Computation.Parsing.Steps
{
    public class CompletedStep<TStep, TData> : IStep<TStep, TData>
    {
        public CompletedStep(bool successful, TStep step)
        {
            Successful = successful;
            Current = step;
        }

        public bool Completed { get; } = true;
        public bool Successful { get; }
        public TStep Current { get; }

        public IStep<TStep, TData> Next(TData data)
        {
            return this;
        }

        private bool Equals(CompletedStep<TStep, TData> other)
        {
            return Successful == other.Successful
                   && EqualityComparer<TStep>.Default.Equals(Current, other.Current);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CompletedStep<TStep, TData>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Successful.GetHashCode() * 397)
                       ^ EqualityComparer<TStep>.Default.GetHashCode(Current);
            }
        }
    }
}