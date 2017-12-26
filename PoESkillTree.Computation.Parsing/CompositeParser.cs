using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Parsing
{
    /// <inheritdoc />
    /// <summary>
    /// Parser that uses <see cref="IStepper{TStep}"/>s to add results of multiple parsers (one for each step) into
    /// a list until a completed step is reached.
    /// <para> The <see cref="IStepper{TStep}"/> is used like a state machine. At each step, the parser for the
    /// current step is executed and the returned value is used as a (boolean) state transition.
    /// </para>
    /// <para> The results of successful parses are added to a list that is output at the end. The output remaining of 
    /// one step serves as the input stat of the next. The last step's remaining is used as the method's output.
    /// <see cref="TryParse"/> returns true iff the Stepper ends in a success step.
    /// </para>
    /// </summary>
    public class CompositeParser<TInnerResult, TStep> : IParser<IReadOnlyList<TInnerResult>>
    {
        private readonly IStepper<TStep> _stepper;
        private readonly Func<TStep, IParser<TInnerResult>> _stepToParserFunc;

        public CompositeParser(IStepper<TStep> stepper, Func<TStep, IParser<TInnerResult>> stepToParserFunc)
        {
            _stepper = stepper;
            _stepToParserFunc = stepToParserFunc;
        }

        public bool TryParse(string stat, out string remaining, out IReadOnlyList<TInnerResult> result)
        {
            var step = _stepper.InitialStep;
            remaining = stat;
            var results = new List<TInnerResult>();
            while (!_stepper.IsTerminal(step))
            {
                IParser<TInnerResult> parser = _stepToParserFunc(step);
                if (parser.TryParse(remaining, out remaining, out var singleResult))
                {
                    results.Add(singleResult);
                    step = _stepper.NextOnSuccess(step);
                }
                else
                {
                    step = _stepper.NextOnFailure(step);
                }
            }

            result = results;
            return _stepper.IsSuccess(step);
        }
    }
}