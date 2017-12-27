namespace PoESkillTree.Computation.Parsing.Data
{
    /// <summary>
    /// State machine that guides through the parsing process. The states/steps can be used to select
    /// the stat matchers that should be matched against (see <see cref="IParsingData{TStep}"/>). 
    /// Transitions have binary input: whether a match with the matchers of the current step could be found or not.
    /// </summary>
    /// <typeparam name="T">The steps of the parsing process, each representing a state.</typeparam>
    public interface IStepper<T>
    {
        /// <summary>
        /// The initial state.
        /// </summary>
        T InitialStep { get; }

        /// <returns>The next state when transitioning from <paramref name="current"/> as current state with a
        /// successful match in the current step. </returns>
        T NextOnSuccess(T current);

        /// <returns>The next state when transitioning from <paramref name="current"/> as current state with no
        /// successful match in the current step. </returns>
        T NextOnFailure(T current);

        /// <returns>True if <paramref name="step"/> is a terminal state, i.e. parsing should be terminated.</returns>
        bool IsTerminal(T step);

        /// <returns>True if <paramref name="step"/> is a terminal state that signals a successful parsing run.
        /// </returns>
        bool IsSuccess(T step);
    }
}