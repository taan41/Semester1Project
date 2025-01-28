using BLL.GameComponents;
using BLL.GameComponents.EntityComponents;
using BLL.GameComponents.ItemComponents;
using BLL.GameComponents.Others;
using DAL;
using DAL.ConfigClasses;

using static System.Console;
using static ConsolePL.ConsoleHelper;

namespace ConsolePL
{
    public class ComponentPrinter
    {
        private static GameConfig GameConfig => ConfigManager.Instance.GameConfig;

        public static void Print<T>(T component) where T : ComponentAbstract
        {
            switch (component)
            {
                case Player player:
                    PrintComponent(player);
                    break;
                case Monster monster:
                    PrintComponent(monster);
                    break;
                case Equipment equip:
                    PrintComponent(equip);
                    break;
                case Skill skill:
                    PrintComponent(skill);
                    break;
                case Gold gold:
                    PrintComponent(gold);
                    break;
                case GameSave save:
                    PrintComponent(save);
                    break;
                default:
                    PrintComponent(component);
                    break;
            }
        }

        public static void PrintComponent(ComponentAbstract component)
        {
            Write($" {component.Name.PadRight(UIConstants.NameLen)}");
            DrawEmptyLine();
        }

        public static void PrintComponent(GameSave save)
        {
            WriteLine($" {save.Name.PadRight(UIConstants.NameLen)} | Saved at: {save.SaveTime:G}  ");
            Write($" Progress: {save.RunData.Progress} - Elapsed Time: {save.RunData.GetElapsedTime():hh\\:mm\\:ss}  ");
            DrawEmptyLine();
        }

        public static void PrintComponent(Player player)
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
            Write($" Gold: {player.PlayerGold.Quantity}");
            DrawEmptyLine();
        }
        
        public static void PrintComponent(Monster monster)
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

        public static void PrintComponent(Equipment equip)
        {
            ForegroundColor = equip.ItemRarity switch
            {
                Item.Rarity.Rare => ConsoleColor.Cyan,
                Item.Rarity.Epic => ConsoleColor.Magenta,
                Item.Rarity.Legendary => ConsoleColor.DarkYellow,
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

        public static void PrintComponent(Skill skill)
        {
            ForegroundColor = skill.ItemRarity switch
            {
                Item.Rarity.Rare => ConsoleColor.Cyan,
                Item.Rarity.Epic => ConsoleColor.Magenta,
                Item.Rarity.Legendary => ConsoleColor.DarkYellow,
                _ => ConsoleColor.White
            };
            Write($" {skill.Name.PadRight(UIConstants.NameLen)}");
            ResetColor();

            Write($"| {skill.SkillType, -6} |");

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
                Write($"[▲ {skill.DamagePoint * GameConfig.SkillPtDamagePercentage * typePercentage / 10000}]");
            }

            if (skill.HealPoint > 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($"[+ {skill.HealPoint * GameConfig.SkillPtHealPercentage / 100}]");
            }

            ForegroundColor = ConsoleColor.Blue;
            Write($" ({skill.MPCost} MP)");
            ResetColor();
            DrawEmptyLine();
        }

        public static void PrintComponent(Gold gold)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" {gold.Quantity} Gold");
            ResetColor();
            DrawEmptyLine();
        }

        public static void PrintPrice<T>(T item, bool buying) where T : Item
        {
            switch (item)
            {
                case Equipment equip:
                    PrintPrice(equip, buying);
                    break;
                case Skill skill:
                    PrintPrice(skill, buying);
                    break;
                default:
                    PrintComponent(item);
                    break;
            }
        }

        public static void PrintPrice(Equipment equip, bool buying)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" ({equip.Price * (buying ? 100 : GameConfig.ItemPriceSellingPercentage) / 100} G)");
            ResetColor();

            ForegroundColor = equip.ItemRarity switch
            {
                Item.Rarity.Rare => ConsoleColor.Cyan,
                Item.Rarity.Epic => ConsoleColor.Magenta,
                Item.Rarity.Legendary => ConsoleColor.DarkYellow,
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

        public static void PrintPrice(Skill skill, bool buying)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" ({skill.Price * (buying ? 100 : GameConfig.ItemPriceSellingPercentage) / 100} G)");
            ResetColor();

            PrintComponent(skill);
        }
    }
}