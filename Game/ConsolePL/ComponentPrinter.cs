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

        // public static void PrintComponent<T>(T component) where T : ComponentAbstract
        // {
        //     PrintComponent(component);
        // }

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
                Write($" [{(equip.BonusATKPoint > 0 ? "+" : "-")}{equip.BonusATKPoint * GameConfig.EquipATKPtPercentage} ATK]");
            }

            if (equip.BonusDEFPoint != 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($" [{(equip.BonusDEFPoint > 0 ? "+" : "-")}{equip.BonusDEFPoint * GameConfig.EquipDEFPtPercentage} DEF]");
            }

            if (equip.BonusHPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Red;
                Write($" [{(equip.BonusHPPoint > 0 ? "+" : "-")}{equip.BonusHPPoint * GameConfig.EquipHPPtPercentage} HP]");
            }

            if (equip.BonusMPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Blue;
                Write($" [{(equip.BonusMPPoint > 0 ? "+" : "-")}{equip.BonusMPPoint * GameConfig.EquipMPPtPercentage} MP]");
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
                ForegroundColor = ConsoleColor.DarkYellow;
                Write($" [▲ {skill.DamagePoint * GameConfig.SkillDamagePtPercentage / 100}]");
            }

            if (skill.HealPoint > 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($" [+ {skill.HealPoint * GameConfig.SkillHealPtPercentage / 100}]");
            }

            ForegroundColor = ConsoleColor.Blue;
            WriteLine($" ({skill.MPCost} MP)");
            ResetColor();
            DrawEmptyLine();
        }

        public static void PrintComponent(Gold gold)
        {
            ForegroundColor = ConsoleColor.Yellow;
            WriteLine($" {gold.Quantity} Gold");
            ResetColor();
            DrawEmptyLine();
        }

        public static void PrintPrice<T>(T item, bool buying) where T : Item
        {
            PrintPrice(item, buying);
        }

        public static void PrintPrice(Equipment equip, bool buying)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" ({equip.Price * (buying ? 100 : GameConfig.ItemSellPricePercentage) / 100} G)");
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
                Write($" [{(equip.BonusATKPoint > 0 ? "+" : "-")}{equip.BonusATKPoint * GameConfig.EquipATKPtPercentage}]");
            }

            if (equip.BonusDEFPoint != 0)
            {
                ForegroundColor = ConsoleColor.Green;
                Write($" [{(equip.BonusDEFPoint > 0 ? "+" : "-")}{equip.BonusDEFPoint * GameConfig.EquipDEFPtPercentage}]");
            }

            if (equip.BonusHPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Red;
                Write($" [{(equip.BonusHPPoint > 0 ? "+" : "-")}{equip.BonusHPPoint * GameConfig.EquipHPPtPercentage}]");
            }

            if (equip.BonusMPPoint != 0)
            {
                ForegroundColor = ConsoleColor.Blue;
                Write($" [{(equip.BonusMPPoint > 0 ? "+" : "-")}{equip.BonusMPPoint * GameConfig.EquipMPPtPercentage}]");
            }

            ResetColor();
            DrawEmptyLine();
        }

        public static void PrintPrice(Skill skill, bool buying)
        {
            ForegroundColor = ConsoleColor.Yellow;
            Write($" ({skill.Price * (buying ? 100 : GameConfig.ItemSellPricePercentage) / 100} G)");
            ResetColor();

            PrintComponent(skill);
        }
    }
}