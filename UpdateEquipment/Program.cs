namespace UpdateEquipment
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // todo Save in Data/Eqipment

            var affixDataLoader = new AffixDataLoader();
            affixDataLoader.Load();
            affixDataLoader.Save("AffixList.xml");

            var itemDataLoader = new ItemDataLoader();
            itemDataLoader.Load();
            itemDataLoader.Save("ItemList.xml");
        }
    }
}
