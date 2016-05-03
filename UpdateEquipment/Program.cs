using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateEquipment
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var affixDataLoader = new AffixDataLoader();
            affixDataLoader.Load();
            affixDataLoader.Save("AffixList.xml");
        }
    }
}
