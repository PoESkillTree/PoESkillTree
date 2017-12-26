using System;
using System.Collections.Generic;
using PoESkillTree.Computation.Parsing.Data;

namespace PoESkillTree.Computation.Data.Steps
{
    public class Stepper : IStepper<ParsingStep>
    {
        private static readonly IReadOnlyDictionary<ParsingStep, ParsingStep> SuccessTransitions =
            new Dictionary<ParsingStep, ParsingStep>
            {
                { ParsingStep.Special, ParsingStep.Success },
                { ParsingStep.StatManipulator, ParsingStep.ValueConversion },
                { ParsingStep.ValueConversion, ParsingStep.FormAndStat },
                { ParsingStep.FormAndStat, ParsingStep.Condition },
                { ParsingStep.Form, ParsingStep.GeneralStat },
                { ParsingStep.GeneralStat, ParsingStep.Condition },
                { ParsingStep.DamageStat, ParsingStep.Condition },
                { ParsingStep.PoolStat, ParsingStep.Condition },
                { ParsingStep.Condition, ParsingStep.Condition }
            };

        private static readonly IReadOnlyDictionary<ParsingStep, ParsingStep> FailureTransitions =
            new Dictionary<ParsingStep, ParsingStep>
            {
                { ParsingStep.Special, ParsingStep.StatManipulator },
                { ParsingStep.StatManipulator, ParsingStep.ValueConversion },
                { ParsingStep.ValueConversion, ParsingStep.FormAndStat },
                { ParsingStep.FormAndStat, ParsingStep.Form },
                { ParsingStep.Form, ParsingStep.Failure },
                { ParsingStep.GeneralStat, ParsingStep.DamageStat},
                { ParsingStep.DamageStat, ParsingStep.PoolStat },
                { ParsingStep.PoolStat, ParsingStep.Failure },
                { ParsingStep.Condition, ParsingStep.Success }
            };

        public ParsingStep InitialStep => ParsingStep.Special;

        public ParsingStep NextOnSuccess(ParsingStep current)
        {
            if (SuccessTransitions.TryGetValue(current, out var next))
            {
                return next;
            }
            switch (current)
            {
                case ParsingStep.Success:
                case ParsingStep.Failure:
                    throw new ArgumentException($"Can't transition from terminal step {current}", nameof(current));
                default:
                    throw new ArgumentOutOfRangeException(nameof(current), current, null);
            }
        }

        public ParsingStep NextOnFailure(ParsingStep current)
        {
            if (FailureTransitions.TryGetValue(current, out var next))
            {
                return next;
            }
            switch (current)
            {
                case ParsingStep.Success:
                case ParsingStep.Failure:
                    throw new ArgumentException($"Can't transition from terminal step {current}", nameof(current));
                default:
                    throw new ArgumentOutOfRangeException(nameof(current), current, null);
            }
        }

        public bool IsTerminal(ParsingStep step) => step == ParsingStep.Success || step == ParsingStep.Failure;

        public bool IsSuccess(ParsingStep step) => step == ParsingStep.Success;
    }
}