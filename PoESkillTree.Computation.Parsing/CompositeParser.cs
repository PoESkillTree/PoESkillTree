using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Steps;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Parser that uses <see cref="IStep{TStep,TData}"/>s to add results of multiple parsers (one for each step) into
    /// a list until a completed step is reached.
    /// <para> The return value of a step is passed to <see cref="IStep{TStep,TData}.Next"/> to advance to the next
    /// step. The remaining output of a step is the input stat for the next step. The last step's remaining output
    /// is the final output and the last step's <see cref="IStep{TStep,TData}.Successful"/> is returned.
    /// </para>
    /// </summary>
    public class CompositeParser<TInnerResult> : IParser<IReadOnlyList<TInnerResult>>
    {
        private readonly IStep<IParser<TInnerResult>, bool> _initialStep;

        public CompositeParser(IStep<IParser<TInnerResult>, bool> initialStep)
        {
            _initialStep = initialStep;
        }

        public bool TryParse(string stat, out string remaining, out IReadOnlyList<TInnerResult> result)
        {
            var step = _initialStep;
            remaining = stat;
            var results = new List<TInnerResult>();
            while (!step.Completed)
            {
                IParser<TInnerResult> parser = step.Current;
                var parserReturn = parser.TryParse(remaining, out remaining, out var singleResult);
                if (parserReturn)
                {
                    results.Add(singleResult);
                }

                step = step.Next(parserReturn);
            }

            result = results;
            return step.Successful;
        }
    }
}