using BLL.Game.Components;
using BLL.Game.Components.Entity;
using BLL.Game.Components.Item;
using BLL.Game.Components.Others;
using BLL.Config;

using static System.Console;
using static ConsolePL.ConsoleUtilities;

namespace ConsolePL
{
    public class ComponentRenderer
    {
        private static GameConfig GameConfig => ConfigManager.Instance.GameConfig;

        public static void Render<T>(T component) where T : GameComponent
            => RenderComponent(component switch
                {
                    Player player => player,
                    Monster monster => monster,
                    Equipment equip => equip,
                    Skill skill => skill,
                    Gold gold => gold,
                    GameSave save => save,
                    _ => component
                });

        public static void RenderComponent(GameComponent component)
        {
            Write($" {component.Name.PadRight(UIConstants.NameLen)}");
            DrawEmptyLine();
        }

        public static void RenderComponent(GameSave save)
        {
            WriteLine($" {save.Name.PadRight(UIConstants.NameLen)} | Saved at: {save.SaveTime:G}  ");
            Write($" Progress: {save.RunData.Progress} - Elapsed Time: {save.RunData.GetElapsedTime():hh\\:mm\\:ss}  ");
            DrawEmptyLine();
        }

        public static void RenderComponent(Player player)
        {
            Write($" {player.Name.PadRight(UIConstants.NameLen)} | ");
            ForegroundColor = ConsoleColor.DarkYellow;
            Write($"▲ (ATK): {player.ATK,-3} ");
            ForegroundColor = ConsoleColor.Green;
            WriteLine($"■ (DEF): {player.DEF,-3}");
            ResetColor();
            Write(" HP ");
            DrawBar(player.HP, player.MaxHP, true, UIConstants.PlayerBarLen, ConsoleColor.Red);
            Write(" MP ");
            DrawBar(player.MP, player.MaxMP, true, UIConstants.PlayerBarLen, ConsoleColor.Blue);
            Write($" Gold: {player.Gold.Quantity}");
            DrawEmptyLine();
        }
        
        public static void RenderComponent(Monster monster)
        {
            int barLen = monster.MonsterType switch
            {
                Monster.Type.Elite => UIConstants.EliteBarLen,
                Monster.Type.Boss => UIConstants.BossBarLen,
                _ => UIConstants.MonsterBarLen
            };
            
            Write($" {monster.Name.PadRight(UIConstants.NameLen)} | ");
            ForegroundColor = ConsoleColor.DarkYellow;
            Write($"▲ {monster.ATK,-2} ");
            ForegroundColor = ConsoleColor.Green;
            Write($"■ {monster.DEF,-2} ");
            ResetColor();
            Write("| ");
            DrawBar(monster.HP, monster.MaxHP, true, barLen, ConsoleColor.Red);
        }

        public static void RenderComponent(Equipment equip)
        {
            ForegroundColor = equip.ItemRarity switch
            {
                GameItem.Rarity.Rare => ConsoleColor.Cyan,
                GameItem.Rarity.Epic => ConsoleColor.Magenta,
                GameItem.Rarity.Legendary => ConsoleColor.DarkYellow,
                _ => ConsoleColor.White
            };
            Write($" {equip.Name.PadRight(UIConstants.NameLen)}");
            ResetColor();

            Write($"| {equip.EquipType, -6} |");

            if (equip.BonusATKPoint != 0)
            {
                ForegroundColor = ConsoleColor.DarkYellow;
                Write($"[{(equip.BonusATKPoint > 0 ? "+" : "-")}{equip.BonusATKPoint * GameConfig.EquipPtATKPercentage / 100} ATK]");
            }

            if (equip.BonusDEFPoint != 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($"[{(equip.BonusDEFPoint > 0 ? "+" : "-")}{equip.BonusDEFPoint * GameConfig.EquipPtDEFPercentage / 100} DEF]");
            }

            if (equip.BonusHPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Red;
                Write($"[{(equip.BonusHPPoint > 0 ? "+" : "-")}{equip.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100} HP]");
            }

            if (equip.BonusMPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Blue;
                Write($"[{(equip.BonusMPPoint > 0 ? "+" : "-")}{equip.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100} MP]");
            }

            ResetColor();
            DrawEmptyLine();
        }

        public static void RenderComponent(Skill skill)
        {
            ForegroundColor = skill.ItemRarity switch
            {
                GameItem.Rarity.Rare => ConsoleColor.Cyan,
                GameItem.Rarity.Epic => ConsoleColor.Magenta,
                GameItem.Rarity.Legendary => ConsoleColor.DarkYellow,
                _ => ConsoleColor.White
            };
            Write($" {skill.Name.PadRight(UIConstants.NameLen)}");
            ResetColor();

            Write($"| {skill.SkillType, -6} |");

            int rarityPercentage = skill.ItemRarity switch
            {
                GameItem.Rarity.Common => GameConfig.SkillRarityCommonPercentage,
                GameItem.Rarity.Rare => GameConfig.SkillRarityRarePercentage,
                GameItem.Rarity.Epic => GameConfig.SkillRarityEpicPercentage,
                GameItem.Rarity.Legendary => GameConfig.SkillRarityLegendaryPercentage,
                _ => 100
            };

            if (skill.DamagePoint > 0)
            {
                int typePercentage = skill.SkillType switch
                {
                    Skill.Type.Single => GameConfig.SkillTypeSinglePercentage,
                    Skill.Type.Random => GameConfig.SkillTypeRandomPercentage,
                    Skill.Type.All => GameConfig.SkillTypeAllPercentage,
                    _ => 0
                };

                ForegroundColor = ConsoleColor.DarkYellow;
                Write($"[▲ {skill.DamagePoint * GameConfig.SkillPtDmgPercentage * rarityPercentage * typePercentage / 1000000}]");
            }

            if (skill.HealPoint > 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($"[+{skill.HealPoint * GameConfig.SkillPtHealPercentage * rarityPercentage / 10000} HP]");
            }
            else if (skill.HealPoint < 0)
            {
                ForegroundColor = ConsoleColor.Red;
                Write($"[-{-skill.HealPoint * GameConfig.SkillPtHealPercentage / 100}HP]");
            }

            ForegroundColor = ConsoleColor.Blue;
            if (skill.MPCost > 0)
                Write($"[-{skill.MPCost}MP]");
            else if (skill.MPCost < 0)
                Write($"[+{-skill.MPCost * rarityPercentage / 100}MP]");
            
            ResetColor();
            DrawEmptyLine();
        }

        public static void RenderComponent(Gold gold)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" {gold.Quantity} Gold");
            ResetColor();
            DrawEmptyLine();
        }

        public static void RenderItemPrice<T>(T item, bool buying) where T : GameItem
        {
            switch (item)
            {
                case Equipment equip:
                    RenderItemPrice(equip, buying);
                    break;
                case Skill skill:
                    RenderItemPrice(skill, buying);
                    break;
                default:
                    RenderComponent(item);
                    break;
            }
        }

        public static void RenderItemPrice(Equipment equip, bool buying)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" ({equip.Price * (buying ? 100 : GameConfig.ItemPriceSellingPercentage) / 100} G)");
            ResetColor();

            ForegroundColor = equip.ItemRarity switch
            {
                GameItem.Rarity.Rare => ConsoleColor.Cyan,
                GameItem.Rarity.Epic => ConsoleColor.Magenta,
                GameItem.Rarity.Legendary => ConsoleColor.DarkYellow,
                _ => ConsoleColor.White
            };
            Write($" {equip.Name.PadRight(UIConstants.NameLen)}");
            ResetColor();

            Write($"| {equip.EquipType, -6} |");

            if (equip.BonusATKPoint != 0)
            {
                ForegroundColor = ConsoleColor.DarkYellow;
                Write($"[{(equip.BonusATKPoint > 0 ? "+" : "-")}{equip.BonusATKPoint * GameConfig.EquipPtATKPercentage / 100}]");
            }

            if (equip.BonusDEFPoint != 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($"[{(equip.BonusDEFPoint > 0 ? "+" : "-")}{equip.BonusDEFPoint * GameConfig.EquipPtDEFPercentage / 100}]");
            }

            if (equip.BonusHPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Red;
                Write($"[{(equip.BonusHPPoint > 0 ? "+" : "-")}{equip.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100}]");
            }

            if (equip.BonusMPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Blue;
                Write($"[{(equip.BonusMPPoint > 0 ? "+" : "-")}{equip.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100}]");
            }

            ResetColor();
            DrawEmptyLine();
        }

        public static void RenderItemPrice(Skill skill, bool buying)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" ({skill.Price * (buying ? 100 : GameConfig.ItemPriceSellingPercentage) / 100} G)");
            ResetColor();

            RenderComponent(skill);
        }
    }
}