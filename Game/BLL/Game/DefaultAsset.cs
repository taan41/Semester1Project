using BLL.Game.Components.Entity;
using BLL.Game.Components.Item;

namespace BLL.Game;

class DefaultAsset
{
    public static Dictionary<int, Equipment> Equipments = new()
    {
        // id, name, rarity, equipType, bonusATKPoint, bonusDEFPoint, bonusHPPoint, bonusMPPoint
        [1] = new(1, "Starter Sword", 0, 0, 1, 1, 0, 0, -1),
        [2] = new(2, "Starter Bow", 0, 0, 2, 0, 0, 0, -1),
        [3] = new(3, "Starter Staff", 0, 0, 1, 0, 0, 1, -1),
        [4] = new(4, "Rusted Iron Sword", 0, 0, 1, 1, 1, 0, -1),
        [5] = new(5, "Cracked Slingshot", 0, 0, 3, 0, 0, 0, -1),
        [6] = new(6, "Worn Staff", 0, 0, 1, 0, 0, 2, -1),
        [7] = new(7, "Rusted Chainmail", 0, 1, 0, 0, 2, 0, -1),
        [8] = new(8, "Torn Leather Vest", 0, 1, 1, 0, 1, 0, -1),
        [9] = new(9, "Apprentice Cloak", 0, 1, 0, 0, 1, 1, -1),
        [10] = new(10, "Tarnished Ring", 0, 2, 0, 0, 0, 1, -1),
        [11] = new(11, "Worn Leather Gloves", 0, 2, 1, 0, 0, 0, -1),
        [12] = new(12, "Cracked Wooden Shield", 0, 2, 0, 1, 0, 0, -1),
        [101] = new(101, "Gleaming Blade", 1, 0, 3, 1, 2, 0, -1),
        [102] = new(102, "Reinforced Crossbow", 1, 0, 5, 0, 1, 0, -1),
        [103] = new(103, "Enchanted Staff", 1, 0, 2, 0, 0, 4, -1),
        [104] = new(104, "Reinforced Scale Mail", 1, 1, 0, 1, 3, 0, -1),
        [105] = new(105, "Sturdy Leather Jacket", 1, 1, 1, 0, 2, 1, -1),
        [106] = new(106, "Enchanted Mantle", 1, 1, 0, 0, 2, 2, -1),
        [107] = new(107, "Tempered Iron Shield", 1, 2, 0, 1, 1, 0, -1),
        [108] = new(108, "Polished Wooden Quiver", 1, 2, 2, 0, 0, 0, -1),
        [109] = new(109, "Blessed Ring", 1, 2, 0, 0, 0, 2, -1),
        [201] = new(201, "Radiant Greatsword", 2, 0, 5, 2, 3, 0, -1),
        [202] = new(202, "Mastercrafted Longbow", 2, 0, 8, 0, 1, 1, -1),
        [203] = new(203, "Runed Staff", 2, 0, 3, 0, 1, 6, -1),
        [204] = new(204, "Royal Half-Plate", 2, 1, 1, 2, 4, 0, -1),
        [205] = new(205, "Royal Light Armour", 2, 1, 3, 1, 2, 1, -1),
        [206] = new(206, "Royal Magic Robe", 2, 1, 0, 1, 2, 4, -1),
        [207] = new(207, "Royal Steel Shield", 2, 2, 0, 2, 1, 0, -1),
        [208] = new(208, "Royal Steel Quiver", 2, 2, 3, 0, 0, 0, -1),
        [209] = new(209, "Royal Enchanted Ring", 2, 2, 0, 0, 0, 3, -1),
        [301] = new(301, "Godforged Warhammer", 3, 0, 8, 3, 4, 0, -1),
        [302] = new(302, "Celestial Bow", 3, 0, 10, 0, 2, 3, -1),
        [303] = new(303, "Divine Orb", 3, 0, 5, 0, 0, 10, -1),
        [304] = new(304, "Godforged Bulwark", 3, 1, 0, 3, 7, 0, -1),
        [305] = new(305, "Celestial Cloak", 3, 1, 5, 1, 2, 2, -1),
        [306] = new(306, "Divine Shroud", 3, 1, 0, 1, 4, 5, -1),
        [307] = new(307, "Godforged Shield", 3, 2, 0, 3, 2, 0, -1),
        [308] = new(308, "Celestial Quiver", 3, 2, 5, 0, 0, 0, -1),
        [309] = new(309, "Divine Ring", 3, 2, 0, 0, 0, 5, -1)
    };

    public static Dictionary<int, Skill> Skills = new()
    {
        // id, name, rarity, skillType, damage, heal, mpCost
        [1] = new(1, "Bandage", 0, 0, 0, 5, 5, -1),
        [2] = new(2, "Weak Stab", 0, 0, 5, 0, 5, -1),
        [3] = new(3, "Careless Slash", 0, 2, 5, 0, 5, -1),
        [101] = new(101, "Cure", 1, 0, 0, 5, 5, -1),
        [102] = new(102, "Piercing Strike", 1, 0, 5, 0, 5, -1),
        [103] = new(103, "Shattering Blow", 1, 2, 5, 0, 5, -1),
        [104] = new(104, "Flameburst", 1, 2, 10, 0, 10, -1),
        [201] = new(201, "Revitalize", 2, 0, 0, 10, 10, -1),
        [202] = new(202, "Precision Thrust", 2, 0, 5, 0, 5, -1),
        [203] = new(203, "Whirlwind Slash", 2, 2, 5, 0, 5, -1),
        [204] = new(204, "Frost Nova", 2, 2, 15, 0, 15, -1),
        [301] = new(301, "Re-life", 3, 0, 0, 10, 10, -1),
        [302] = new(302, "Godslayer Strike", 3, 0, 5, 0, 5, -1),
        [303] = new(303, "Earthshatter", 3, 2, 5, 0, 5, -1),
        [304] = new(304, "Meteor Storm", 3, 2, 20, 0, 20, -1)
    };

    public static Dictionary<int, Monster> Monsters = new()
    {
        // id, name, floor, type, atk, def, hp (no need for maxhp, mp & maxmp)
        [1] = new(1, "Slime", 1, 0, 10, 0, 100),
        [2] = new(2, "Rat", 1, 0, 15, 0, 100),
        [101] = new(101, "Mother Slime", 1, 1, 15, 1, 100),
        [201] = new(201, "King Slime", 1, 2, 10, 3, 100),
        [1001] = new(1001, "Goldfish", 2, 0, 10, 1, 100),
        [1101] = new(1101, "Shark", 2, 1, 10, 3, 100),
        [1201] = new(1201, "Ocean God Neptune", 2, 2, 10, 6, 100),
        [2001] = new(2001, "Skeleton", 3, 0, 10, 2, 100),
        [2101] = new(2101, "Grave Digger", 3, 1, 10, 5, 100),
        [2201] = new(2201, "Supreme Necromancer", 3, 2, 10, 9, 100)
    };
}