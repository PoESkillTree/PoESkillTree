using PoESkillTree.Computation.Parsing;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            IParser<string> parser = new ToStringParserDecorator<string>(new DumpParser());

            System.Console.Write("> ");
            string statLine;
            while ((statLine = System.Console.ReadLine()) != "")
            {
                if (!parser.TryParse(statLine, out var result))
                {
                    System.Console.WriteLine("Not recognized, printing partial result");
                }
                System.Console.WriteLine(result);
                System.Console.Write("> ");
            }
        }

        // Obviously only temporary until the actually useful classes exist
        private class DumpParser : IParser<string>
        {
            public bool TryParse(string stat, out string result)
            {
                result = stat;
                return true;
            }
        }
    }
}
