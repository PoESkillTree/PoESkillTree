using System.Collections.Generic;
using System.Linq;
using NLog;

namespace PoESkillTree.Model.Serialization.PathOfBuilding
{
    public static class ConfigurationStatConverter
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        public static IEnumerable<(string, double?)> Convert(IEnumerable<XmlPathOfBuildingConfigInput> xmlConfigs) =>
            xmlConfigs.Select(Convert).Where(n => n.HasValue).Select(n => n!.Value);

        private static (string, double?)? Convert(XmlPathOfBuildingConfigInput xmlConfig) =>
            xmlConfig.Name switch
            {
                "buffConflux" => ("Character.ShaperOfDesolationStep", xmlConfig.Number),
                "buffFortify" => ("Character.Fortify.ExplicitlyActive", ConvertBoolean(xmlConfig)),
                "buffOnslaught" => ("Character.Onslaught.ExplicitlyActive", ConvertBoolean(xmlConfig)),
                "buffPendulum" => ("Character.PendulumOfDestructionStep", ConvertPendulumOfDestruction(xmlConfig)),
                "buffTailwind" => ("Character.Tailwind.ExplicitlyActive", ConvertBoolean(xmlConfig)),
                "conditionAtCloseRange" => ("Enemy.IsNearby", ConvertBoolean(xmlConfig)),
                "conditionAttackedRecently" => ("Character.Skills[Attack].Cast.RecentOccurrences", ConvertBoolean(xmlConfig)),
                "conditionCastSpellRecently" => ("Character.Skills[Spell].Cast.RecentOccurrences", ConvertBoolean(xmlConfig)),
                "conditionCritRecently" => ("Character.CriticalStrike.RecentOccurrences", ConvertBoolean(xmlConfig)),
                "conditionEnemyBleeding" => ("Enemy.Bleed.Active", ConvertBoolean(xmlConfig)),
                "conditionEnemyBlinded" => ("Character.Blind.ExplicitlyActiveOnEnemy", ConvertBoolean(xmlConfig)),
                "conditionEnemyChilled" => ("Enemy.Chill.Active", ConvertBoolean(xmlConfig)),
                "conditionEnemyIgnited" => ("Enemy.Ignite.Active", ConvertBoolean(xmlConfig)),
                "conditionEnemyIntimidated" => ("Character.Intimidate.ExplicitlyActiveOnEnemy", ConvertBoolean(xmlConfig)),
                "conditionEnemyMaimed" => ("Character.Maim.ExplicitlyActiveOnEnemy", ConvertBoolean(xmlConfig)),
                "conditionEnemyShocked" => ("Enemy.Shock.Active", ConvertBoolean(xmlConfig)),
                "conditionEnemyTaunted" => ("Character.Taunt.ExplicitlyActiveOnEnemy", ConvertBoolean(xmlConfig)),
                "conditionFullLife" => ("Character.Life.IsFull", ConvertBoolean(xmlConfig)),
                "conditionHaveTotem" => ("Character.Skills[Totem].Instances", ConvertBoolean(xmlConfig)),
                "conditionKilledRecently" => ("Character.Kill.RecentOccurrences", ConvertBoolean(xmlConfig)),
                "conditionLeeching" => ("Character.Leech.IsActive", ConvertBoolean(xmlConfig)),
                "conditionOnConsecratedGround" => ("Character.ConsecratedGround.Active", ConvertBoolean(xmlConfig)),
                "conditionShockEffect" => ("Enemy.Shock.IncreasedDamageTaken", xmlConfig.Number),
                "conditionShockedEnemyRecently" => ("Character.Shock.Infliction.RecentOccurences", ConvertBoolean(xmlConfig)),
                "conditionTauntedEnemyRecently" => ("Character.Taunt.Infliction.RecentOccurences", ConvertBoolean(xmlConfig)),
                "conditionUsedSkillRecently" => ("Character.Skills[].Cast.RecentOccurrences", ConvertBoolean(xmlConfig)),
                "critChanceLucky" => ("Character.CriicalStrikeChanceIsLucky", ConvertBoolean(xmlConfig)),
                "enemyConditionHitByLightningDamage" => ("Character.LightningHit.RecentOccurences", ConvertBoolean(xmlConfig)),
                "enemyIsBoss" => ("Character.SelectedBossType", ConvertSelectedBossType(xmlConfig)),
                "multiplierMineDetonatedRecently" => ("Character.# of Mines Detonated Recently", xmlConfig.Number),
                "multiplierNearbyEnemies" => ("Enemy.CountNearby", xmlConfig.Number),
                "multiplierPoisonOnEnemy" => ("Enemy.Poison.InstanceCount", xmlConfig.Number),
                "multiplierSkillUsedRecently" => ("Character.Skills[].Cast.RecentOccurrences", xmlConfig.Number),
                "projectileDistance" => ("Character.Projectile.TravelDistance", xmlConfig.Number),
                "skillChainCount" => ("Character.Projectile.ChainedCount", xmlConfig.Number),
                "useEnduranceCharges" => ("Character.Endurance.Charge.Amount.SetToMaximum", ConvertBoolean(xmlConfig)),
                "useFrenzyCharges" => ("Character.Frenzy.Charge.Amount.SetToMaximum", ConvertBoolean(xmlConfig)),
                "useInspirationCharges" => ("Character.Inspiration.Charge.Amount.SetToMaximum", ConvertBoolean(xmlConfig)),
                "usePowerCharges" => ("Character.Power.Charge.Amount.SetToMaximum", ConvertBoolean(xmlConfig)),
                _ => Unknown(xmlConfig)
            };

        private static double? ConvertPendulumOfDestruction(XmlPathOfBuildingConfigInput xmlConfig) =>
            xmlConfig.String switch
            {
                "AREA" => 1,
                "DAMAGE" => 2,
                _ => 0
            };

        private static double? ConvertSelectedBossType(XmlPathOfBuildingConfigInput xmlConfig) =>
            xmlConfig.String == "SHAPER" ? 2 : (xmlConfig.Boolean ? 1 : 0);

        private static double? ConvertBoolean(XmlPathOfBuildingConfigInput xmlConfig) =>
            xmlConfig.Boolean ? 1 : (double?) null;

        private static (string, double?)? Unknown(XmlPathOfBuildingConfigInput xmlConfig)
        {
            Log.Info($"Unknown configuration stat: {xmlConfig.Name}"
                     + $" (bool: {xmlConfig.Boolean}, number: {xmlConfig.Number}, string: {xmlConfig.String})");
            return null;
        }
    }
}