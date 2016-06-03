using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using POESKillTree.Localization;
using POESKillTree.Utils;

namespace POESKillTree.ItemFilter.Model
{
    public class FilterManager
    {
        private const string GameDocumentsFolder = @"My Games\Path of Exile";

        private const string GameFilterMagicLine = "# PoESkillTree";

        private static Regex ReInvalidFileName = new Regex(@"^\s|\s$|[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");

        public static Filter Create(string name)
        {
            Filter filter = new Filter
            {
                Name = name,
                Groups = new List<RuleGroup>
                {
                    new RuleGroup
                    {
                        Id = "Currency",
                        Name = L10n.Message("Currency"),
                        Matches = new List<Match> { new MatchClass(new string[]{ "Currency" }) },
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                Id = "Wisdom",
                                Name = "Scroll of Wisdom",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Scroll of Wisdom" }) }
                            },
                            new Rule
                            {
                                Id = "Portal",
                                Name = "Portal Scroll",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Portal Scroll" }) }
                            },
                            new Rule
                            {
                                Id = "Scrap",
                                IsEnabled = false,
                                Name = "Armourer's Scrap",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Armourer's Scrap" }) }
                            },
                            new Rule
                            {
                                Id = "Whetstone",
                                IsEnabled = false,
                                Name = "Blacksmith's Whetstone",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Blacksmith's Whetstone" }) }
                            },
                            new Rule
                            {
                                Id = "Bauble",
                                IsEnabled = false,
                                Name = "Glassblower's Bauble",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Glassblower's Bauble" }) }
                            },
                            new Rule
                            {
                                Id = "Transmutation",
                                IsEnabled = false,
                                Name = "Orb of Transmutation",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Transmutation" }) }
                            },
                            new Rule
                            {
                                Id = "Augmentation",
                                IsEnabled = false,
                                Name = "Orb of Augmentation",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Augmentation" }) }
                            },
                            new Rule
                            {
                                Id = "Chance",
                                IsEnabled = false,
                                Name = "Orb of Chance",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Chance" }) }
                            },
                            new Rule
                            {
                                Id = "Alteration",
                                IsEnabled = false,
                                Name = "Orb of Alteration",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Alteration" }) }
                            },
                            new Rule
                            {
                                Id = "Chromatic",
                                IsEnabled = false,
                                Name = "Chromatic Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Chromatic Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Jeweller",
                                IsEnabled = false,
                                Name = "Jeweller's Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Jeweller's Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Fusing",
                                IsEnabled = false,
                                Name = "Orb of Fusing",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Fusing" }) }
                            },
                            new Rule
                            {
                                Id = "Chisel",
                                IsEnabled = false,
                                Name = "Cartographer's Chisel",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Cartographer's Chisel" }) }
                            },
                            new Rule
                            {
                                Id = "Alchemy",
                                IsEnabled = false,
                                Name = "Orb of Alchemy",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Alchemy" }) }
                            },
                            new Rule
                            {
                                Id = "Chaos",
                                IsEnabled = false,
                                Name = "Chaos Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Chaos Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Scouring",
                                IsEnabled = false,
                                Name = "Orb of Scouring",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Scouring" }) }
                            },
                            new Rule
                            {
                                Id = "Regret",
                                IsEnabled = false,
                                Name = "Orb of Regret",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Orb of Regret" }) }
                            },
                            new Rule
                            {
                                Id = "Vaal",
                                IsEnabled = false,
                                Name = "Vaal Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Vaal Orb" }) }
                            },
                            new Rule
                            {
                                Id = "GCP",
                                IsEnabled = false,
                                Name = "Gemcutter's Prism",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Gemcutter's Prism" }) }
                            },
                            new Rule
                            {
                                Id = "Blessed",
                                IsEnabled = false,
                                Name = "Blessed Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Blessed Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Regal",
                                IsEnabled = false,
                                Name = "Regal Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Regal Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Exalt",
                                IsEnabled = false,
                                Name = "Exalted Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Exalted Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Divine",
                                IsEnabled = false,
                                Name = "Divine Orb",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Divine Orb" }) }
                            },
                            new Rule
                            {
                                Id = "Mirror",
                                IsEnabled = false,
                                Name = "Mirror of Kalandra",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Mirror of Kalandra" }) }
                            },
                            new Rule // XXX: Prophecy league
                            {
                                Id = "SilverCoin",
                                IsEnabled = false,
                                Name = "Silver Coin",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Silver Coin" }) }
                            }
                        }
                    },
                    new RuleGroup
                    {
                        Id = "Recipes",
                        Name = L10n.Message("Recipes"),
                        Rules = new List<Rule>
                        {
                            //new Rule { Name = "Scroll Fragment", Group = recipeGroup },   // Normal, Gem, Flask, ...
                            //new Rule { Name = "Shards from Magic items", Group = recipeGroup },   // Magic (Transmutation Shard, Alteration Shard, Alchemy Shard)
                            //new Rule { Name = "Shards from Rare items", Group = recipeGroup },   // Rare (Transmutation Shard, Alteration Shard, Alchemy Shard)
                            new Rule
                            {
                                Id = "Whetstone",
                                IsEnabled = false,
                                Name = "Blacksmith's Whetstone",
                                Description = L10n.Message("A single Normal weapon with 20% quality.\nWeapons with a total of at least 40% quality."),
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Claws", "Daggers", "Wands", "Swords", "Axes", "Maces", "Bows", "Staves", "Sceptres" }),
                                    new MatchRarity(MatchEnum.Operator.LessOrEqual, MatchRarity.Rarity.Magic),
                                    new MatchQuality(MatchNumber.Operator.GreaterThan, 0)
                                }
                            },
                            new Rule
                            {
                                Id = "Scrap",
                                IsEnabled = false,
                                Name = "Armourer's Scrap",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Gloves", "Boots", "Body Armours", "Helmets", "Shields" }),
                                    new MatchRarity(MatchEnum.Operator.LessOrEqual, MatchRarity.Rarity.Magic),
                                    new MatchQuality(MatchNumber.Operator.GreaterThan, 0)
                                }
                            },
                            new Rule
                            {
                                Id = "Bauble",
                                Name = "Glassblower's Bauble",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Flasks" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique),
                                    new MatchQuality(MatchNumber.Operator.GreaterThan, 0)
                                }
                            },
                            new Rule
                            {
                                Id = "Chromatic",
                                IsEnabled = false,
                                Name = "Chromatic Orb",
                                Matches = new List<Match> { new MatchSocketGroup(new string[]{ "RGB" }) }
                            },
                            new Rule
                            {
                                Id = "Jeweller",
                                IsEnabled = false,
                                Name = "Jeweller's Orb",
                                Matches = new List<Match> { new MatchSockets(MatchNumber.Operator.Equal, 6) }
                            },
                            new Rule
                            {
                                Id = "Chisel",
                                IsEnabled = false,
                                Name = "Cartographer's Chisel",
                                Set = new List<Match>[]
                                {
                                    // XXX: 3 or less whetstones to be spent to bring an item to quality of 20 for a recipe.
                                    new List<Match>
                                    {
                                        new MatchBaseType(new string[]{ "Stone Hammer", "Rock Breaker", "Gavel" }),
                                        new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal),
                                        new MatchQuality(MatchNumber.Operator.GreaterOrEqual, 5)
                                    },
                                    new List<Match>
                                    {
                                        new MatchBaseType(new string[]{ "Stone Hammer", "Rock Breaker", "Gavel" }),
                                        new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Magic),
                                        new MatchQuality(MatchNumber.Operator.GreaterOrEqual, 14)
                                    },
                                    new List<Match>
                                    {
                                        new MatchBaseType(new string[]{ "Stone Hammer", "Rock Breaker", "Gavel" }),
                                        new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Rare),
                                        new MatchQuality(MatchNumber.Operator.GreaterOrEqual, 17)
                                    }
                                }
                            },
                            new Rule
                            {
                                Id = "Chaos",
                                IsEnabled = false,
                                Name = "Chaos Orb",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Claws", "Daggers", "Wands", "Swords", "Axes", "Maces", "Bows", "Staves", "Sceptres", "Gloves", "Boots", "Body Armours", "Helmets", "Shields", "Rings", "Amulets" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Rare),
                                    new MatchItemLevel(MatchNumber.Operator.Between, 60, 74)
                                }
                            },
                            new Rule
                            {
                                Id = "Regal",
                                IsEnabled = false,
                                Name = "Regal Orb",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Claws", "Daggers", "Wands", "Swords", "Axes", "Maces", "Bows", "Staves", "Sceptres", "Gloves", "Boots", "Body Armours", "Helmets", "Shields", "Rings", "Amulets" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Rare),
                                    new MatchItemLevel(MatchNumber.Operator.GreaterOrEqual, 75)
                                }
                            },
                            new Rule
                            {
                                Id = "GCP",
                                IsEnabled = false,
                                Name = "Gemcutter's Prism",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Gems" }),
                                    new MatchQuality(MatchNumber.Operator.GreaterThan, 0)
                                }
                            },
                            new Rule
                            {
                                Id = "Divine",
                                IsEnabled = false,
                                Name = "Divine Orb",
                                Matches = new List<Match> { new MatchLinkedSockets(MatchNumber.Operator.Equal, 6) }
                            }
                        }
                    },
                    new RuleGroup
                    {
                        Id = "Gems",
                        Name = L10n.Message("Gems"),
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                Id = "Skill",
                                Name = "Skill Gem",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Active Skill Gems" }) }
                            },
                            new Rule
                            {
                                Id = "Support",
                                Name = "Support Gem",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Support Skill Gems" }) }
                            },
                            new Rule
                            {
                                Id = "Vaal",
                                Name = "Vaal Gem",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Gems" }),
                                    new MatchBaseType(new string[]{ "Vaal" })
                                }
                            }
                        }
                    },
                    new RuleGroup
                    {
                        Id = "Flasks",
                        Name = L10n.Message("Flasks"),
                        Matches = new List<Match>
                        {
                            new MatchClass(new string[]{ "Flasks" }),
                            new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique),
                            new MatchQuality() // Implicit.
                        },
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                Id = "Life",
                                Name = "Life Flask",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Life Flasks" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Mana",
                                Name = "Mana Flask",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Mana Flasks" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Hybrid",
                                Name = "Hybrid Flask",
                                Matches = new List<Match>
                                {
                                    new MatchClass(new string[]{ "Hybrid Flasks" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Amethyst",
                                Name = "Amethyst Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Amethyst Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Ruby",
                                Name = "Ruby Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Ruby Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Sapphire",
                                Name = "Sapphire Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Sapphire Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Topaz",
                                Name = "Topaz Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Topaz Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Granite",
                                Name = "Granite Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Granite Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Jade",
                                Name = "Jade Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Jade Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Quicksilver",
                                Name = "Quicksilver Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Quicksilver Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Quartz",
                                Name = "Quartz Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Quartz Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Diamond",
                                Name = "Diamond Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Diamond Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Aquamarine",
                                Name = "Aquamarine Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Aquamarine Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Basalt",
                                Name = "Basalt Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Basalt Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Bismuth",
                                Name = "Bismuth Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Bismuth Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Silver",
                                Name = "Silver Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Silver Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Stibnite",
                                Name = "Stibnite Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Stibnite Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            },
                            new Rule
                            {
                                Id = "Sulphur",
                                Name = "Sulphur Flask",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Sulphur Flask" }),
                                    new MatchRarity(MatchEnum.Operator.LessThan, MatchRarity.Rarity.Unique)
                                }
                            }
                        }
                    },
                    new RuleGroup
                    {
                        Id = "Miscellaneous",
                        Name = L10n.Message("Miscellaneous"),
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                Id = "5L",
                                IsEnabled = false,
                                Name = "5 Linked Sockets",
                                Matches = new List<Match> { new MatchLinkedSockets(MatchNumber.Operator.Equal, 5) }
                            },
                            new Rule
                            {
                                Id = "Card",
                                IsEnabled = false,
                                Name = "Divination Card",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Divination Card" }) }
                            },
                            new Rule
                            {
                                Id = "FishingRod",
                                IsEnabled = false,
                                Name = "Fishing Rod",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Fishing Rods" }) }
                            },
                            new Rule
                            {
                                Id = "Jewel",
                                IsEnabled = false,
                                Name = "Jewel",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Jewel" }) }
                            },
                            new Rule
                            {
                                Id = "LabyrinthKey",
                                IsEnabled = false,
                                Name = "Labyrinth Key",
                                Matches = new List<Match> { new MatchBaseType(new string[]{ "Golden Key", "Silver Key", "Treasure Key" }) }
                            },
                            new Rule // XXX: Prophecy league
                            {
                                Id = "LabyrinthMap",
                                IsEnabled = false,
                                Name = "Labyrinth Map",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Labyrinth Map Item" }) }
                            },
                            new Rule
                            {
                                Id = "LabyrinthTrinket",
                                IsEnabled = false,
                                Name = "Labyrinth Trinket",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Trinket" }) }
                            },
                            new Rule
                            {
                                Id = "Maps",
                                IsEnabled = false,
                                Name = "Map",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Maps", "Map Fragments" }) }
                            },
                            new Rule
                            {
                                Id = "QuestItems",
                                IsEnabled = false,
                                Name = "Quest Item",
                                Matches = new List<Match> { new MatchClass(new string[]{ "Quest Items" }) }
                            }
                        }
                    },
                    new RuleGroup
                    {
                        Id = "Chancing",
                        Name = L10n.Message("Chancing"),
                        Rules = new List<Rule>
                        {
                            new Rule
                            {
                                Id = "Headhunter",
                                IsEnabled = false,
                                Name = "Headhunter",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Leather Belt" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "HegemonysEraPledgeOfHands",
                                IsEnabled = false,
                                Name = "Hegemony's Era, Pledge of Hands",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Judgement Staff" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "KaomsHeart",
                                IsEnabled = false,
                                Name = "Kaom's Heart",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Glorious Plate" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "Mjolner",
                                IsEnabled = false,
                                Name = "Mjölner",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Gavel" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "ShavronnesRevelation",
                                IsEnabled = false,
                                Name = "Shavronne's Revelation",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Moonstone Ring" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "ShavronnesWrappings",
                                IsEnabled = false,
                                Name = "Shavronne's Wrappings",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Occultist's Vestment" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "Skyforth",
                                IsEnabled = false,
                                Name = "Skyforth",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Sorcerer Boots" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "SoulTaker",
                                IsEnabled = false,
                                Name = "Soul Taker",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Siege Axe" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "VoidBattery",
                                IsEnabled = false,
                                Name = "Void Battery",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Prophecy Wand" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "VollsDevotion",
                                IsEnabled = false,
                                Name = "Voll's Devotion",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Agate Amulet" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "Voltaxic",
                                IsEnabled = false,
                                Name = "Voltaxic Rift",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Spine Bow" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            },
                            new Rule
                            {
                                Id = "Windripper",
                                IsEnabled = false,
                                Name = "Windripper",
                                Matches = new List<Match>
                                {
                                    new MatchBaseType(new string[]{ "Imperial Bow" }),
                                    new MatchRarity(MatchEnum.Operator.Equal, MatchRarity.Rarity.Normal)
                                }
                            }
                        }
                    }
                }
            };

            filter.Refresh();

            // Learn from relationship between group and its rules.
            foreach (RuleGroup group in filter.Groups)
                group.Learn();

            return filter;
        }

        public static void Delete(string name)
        {
            string path = PathOf(name);

            if (File.Exists(path))
                File.Delete(path);

            path = GamePathOf(name);
            if (File.Exists(path))
                File.Delete(path);
        }

        public static void Enable(Filter filter)
        {
            string path = GamePathOf(filter.Name);

            // XXX: Optimize before opening stream to not overwrite generated filter in case of error.
            List<Block> output = Optimize(filter.GetBlocks());

            using(StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                // Write magic string for possible identification of filters generated by us.
                writer.WriteLine(GameFilterMagicLine);

                foreach (Block block in output)
                    foreach (string line in block.GetLines())
                        writer.WriteLine(line);
            }
        }

        /// <summary>
        /// Determines whether filter with given name already exists.
        /// </summary>
        /// <param name="name">The filter name.</param>
        /// <returns>true if filter exists, otherwise false.</returns>
        public static bool Exists(string name)
        {
            return File.Exists(PathOf(name));
        }
        
        /// <summary>
        /// Determines whether filter with given name already exists in game folder.
        /// </summary>
        /// <param name="name">The filter name.</param>
        /// <returns>true if filter exists, otherwise false.</returns>
        public static bool GameFilterExists(string name)
        {
            return File.Exists(GamePathOf(name));
        }

        /// <summary>
        /// Returns file path of game filter.
        /// </summary>
        /// <param name="name">The filter name.</param>
        /// <returns>The file path.</returns>
        private static string GamePathOf(string name)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), GameDocumentsFolder, name + ".filter");
        }

        /// <summary>
        /// Determines whether filter file in game folder was created by us.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool IsGameFilterOurs(string name)
        {
            // A file might be empty.
            try
            {
                // Get first line of game filter file.
                string magic = File.ReadLines(GamePathOf(name)).First();

                return string.Equals(magic, GameFilterMagicLine);
            }
            catch (Exception) { }

            return false;
        }

        public static bool IsValidFilterName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && !ReInvalidFileName.IsMatch(name);
        }

        public static Filter Load(string name)
        {
            string path = PathOf(name);

            using(StreamReader reader = new StreamReader(path))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Filter.FilterState));
                Filter.FilterState state = (Filter.FilterState) serializer.Deserialize(reader);

                Filter filter = Create(name);
                filter.Restore(state);

                // Expand groups with unchecked rules or modified colors.
                foreach (RuleGroup group in filter.Groups)
                    if (group.Rules.Exists(r => !r.IsChecked || r.HasColors))
                        group.IsExpanded = true;

                return filter;
            }
        }

        private static List<Block> Optimize(List<Block> blocks)
        {
            List<Block> output = new List<Block>();

            // Phase 1: Visibility (i.e. show only those blocks which have colors defined or lower priority block would hide them).

            List<Block> input = new List<Block>(blocks);
            input.Sort();

            while (input.Count > 0)
            {
                // Fetch first block from input for processing.
                Block block = input[0];
                input.Remove(block);

                // Show block which has no colors defined is kept only if it subsets lower or same priority block with different visual.
                if (block.Show && !block.HasColors)
                {
                    if (input.Exists(b => block.Subsets(b) && !b.VisualEquals(block)))
                        output.Add(block);
                }
                else if (block.Show && block.HasColors)
                {
                    // Show block which has colors defined is kept only if it doesn't subset lower or same priority block with same visual.
                    if (!input.Exists(b => block.Subsets(b) && b.VisualEquals(block)))
                        output.Add(block);
                }
                else if (!block.Show)
                {
                    // Hide block is kept only if it subsets lower or same priority Show block from same RuleGroup
                    if (input.Exists(b => block.Subsets(b) && b.Show && b.OfGroup == block.OfGroup)
                        // or it doesn't subset any other lower or same priority block.
                        || !input.Exists(b => block.Subsets(b)))
                        output.Add(block);
                }
                else
                    output.Add(block);
            }

            // Phase 2: Redundancy (i.e. try to merge blocks with same visual and same priority).
            input = new List<Block>(output);
            output.Clear();

            // Iterate through blocks in reversed order (i.e. from lowest priority to highest).
            input.Sort();
            input.Reverse();
            while (input.Count > 0)
            {
                // Fetch first block from input for processing.
                Block block = input[0];
                input.Remove(block);

                // Find input blocks which can be merged into block being processed.
                foreach (Block merge in input.FindAll(b => block.CanMerge(b)))
                {
                    // Merge block and remove it from input.
                    block.Merge(merge);
                    input.Remove(merge);
                }

                output.Add(block);
            }

            output.Sort();

            return output;
        }

        private static string PathOf(string name)
        {
            return AppData.GetFolder("Filters", true) + name + ".xml";
        }

        public static void Save(Filter filter)
        {
            string path = PathOf(filter.Name);

            using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Filter.FilterState));
                serializer.Serialize(writer, filter.Store());
            }
        }

        public static List<string> GetFilterNames()
        {
            List<string> names = new List<string>();

            DirectoryInfo dir = new DirectoryInfo(AppData.GetFolder("Filters"));

            foreach (FileInfo file in dir.GetFiles("*.xml"))
                names.Add(file.Name.Substring(0, file.Name.Length - file.Extension.Length));

            return names;
        }
    }
}
