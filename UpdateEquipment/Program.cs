using System;
using System.Diagnostics;
using System.IO;
using POESKillTree.Utils;

namespace UpdateEquipment
{
    public static class Program
    {
        // current directory while debugging is UpdateEquipment/bin/Debug/
        private static readonly string PathForDebug = Path.Combine("..", "..", "..", "WPFSKillTree");

        public static void Main(string[] args)
        {
            var savePath = Path.Combine(Debugger.IsAttached ? PathForDebug : AppData.GetFolder(), "Data", "Equipment");
            Load("affixes", Path.Combine(savePath, "AffixList.xml"), new AffixDataLoader());
            Load("base items", Path.Combine(savePath, "ItemList.xml"), new ItemDataLoader());
        }

        private static void Load<T>(string name, string path, DataLoader<T> dataLoader)
        {
            Console.Write("Loading " + name + " ...");
            dataLoader.Load();
            File.Copy(path, path + ".bak", true);
            dataLoader.Save(path);
            Console.WriteLine(" Done!");
        }
    }
}
