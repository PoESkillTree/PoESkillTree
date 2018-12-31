using System.IO;
using NUnit.Framework;

namespace PoESkillTree.GameModel.Tests
{
    public static class TestUtils
    {
        public static string ReadDataFile(string fileName)
            => File.ReadAllText(GetDataFilePath(fileName));

        public static string GetDataFilePath(string filename)
            => TestContext.CurrentContext.TestDirectory + "/Data/" + filename;
    }
}