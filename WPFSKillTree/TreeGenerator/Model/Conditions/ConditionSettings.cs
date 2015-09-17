namespace POESKillTree.TreeGenerator.Model.Conditions
{
    public class ConditionSettings
    {
        public WeaponClass WeaponClass { get; private set; }

        public Tags Tags { get; private set; }

        public OffHand OffHand { get; private set; }
        
        public string[] Keystones { get; private set; }

        public ConditionSettings(Tags tags, OffHand offHand, string[] keystones, WeaponClass weaponClass)
        {
            Tags = tags;
            OffHand = offHand;
            Keystones = keystones;
            WeaponClass = weaponClass;
        }
    }
}