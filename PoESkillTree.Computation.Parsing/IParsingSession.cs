namespace PoESkillTree.Computation.Parsing
{
    public interface IParsingSession<TParserResult>
    {
        bool Completed { get; }

        bool Successful { get; }

        IParser<TParserResult> CurrentParser { get; }

        void ParseSuccessful();
        void ParseFailed();
    }
}