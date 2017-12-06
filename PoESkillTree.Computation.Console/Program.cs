using PoESkillTree.Computation.Parsing;
using PoESkillTree.Computation.Parsing.Referencing;

namespace PoESkillTree.Computation.Console
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var compositionRoot = new CompositionRoot();

            ReferenceValidator.Validate(compositionRoot.ReferencedMatchers, compositionRoot.StatMatchers);

            var parser = compositionRoot.CreateParser();

            System.Console.WriteLine("Enter a stat line");
            System.Console.Write("> ");
            string statLine;
            while ((statLine = System.Console.ReadLine()) != "")
            {
                try
                {
                    if (!parser.TryParse(statLine, out var remaining, out var result))
                    {
                        System.Console.WriteLine($"Not recognized: '{remaining}' could not be parsed.");
                    }
                    System.Console.WriteLine(result == null ? "null" : string.Join("\n", result));
                }
                catch (ParseException e)
                {
                    System.Console.WriteLine("Parsing failed: " + e.Message);
                }
                System.Console.Write("> ");
            }
        }
    }
}
