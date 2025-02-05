using System.Text.Json;
using System.Text.Json.Serialization;
using BLL.Game.Components.Item;
using BLL.Config;

namespace BLL.Game.Components.Entity
{
    [Serializable]
    [JsonConverter(typeof(PlayerJsonConverter))]
    public class Player : GameEntity
    {
        private static int MaxSkillCount => GameConfig.PlayerMaxSkillCount;

        public List<Equipment> Equipped { get; set; } = [];
        public List<Equipment> EquipInventory { get; set; } = [];
        public List<Skill> Skills { get; set; } = [];
        public List<Skill> SkillInventory { get; set; } = [];
        public Gold Gold { get; set; } = new(0);

        public Player() {}

        public Player(string name, int atk, int def, int hp, int mp, int goldQuantity) : base(name, atk, def, hp, mp)
        {
            Gold = new(goldQuantity);
        }

        public static Player DefaultPlayer() 
            => new("Player", GameConfig.PlayerDefaultATK, GameConfig.PlayerDefaultDEF, GameConfig.PlayerDefaultHP, GameConfig.PlayerDefaultMP, GameConfig.PlayerDefaultGold);

        public void ChangeEquip(Equipment equipToChange)
        {
            Equipment? oldEquipment = null;

            Equipped.ForEach(equip =>
            {
                if (equip.EquipType == equipToChange.EquipType)
                    oldEquipment = equip;
            });
            
            ATK += equipToChange.BonusATKPoint * GameConfig.EquipPtATKPercentage / 100;
            DEF += equipToChange.BonusDEFPoint * GameConfig.EquipPtDEFPercentage / 100;
            MaxHP += equipToChange.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100;
            HP += equipToChange.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100;
            MaxMP += equipToChange.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100;
            MP += equipToChange.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100;

            Equipped.Add(equipToChange);
            EquipInventory.Remove(equipToChange);
            
            if (oldEquipment != null)
            {
                ATK -= oldEquipment.BonusATKPoint * GameConfig.EquipPtATKPercentage / 100;
                DEF -= oldEquipment.BonusDEFPoint * GameConfig.EquipPtDEFPercentage / 100;
                MaxHP -= oldEquipment.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100;
                int newHP = HP - oldEquipment.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100;
                HP = newHP < 1 ? 1 : newHP;
                MaxMP -= oldEquipment.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100;
                MP -= oldEquipment.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100;
                
                Equipped.Remove(oldEquipment);
                EquipInventory.Add(oldEquipment);
            }

            Equipped.Sort(new EquipmentComparer());
            EquipInventory.Sort(new EquipmentComparer());
        }

        public void ChangeSkill(int index, Skill skillToChange)
        {
            Skill removedSkill = Skills.ElementAt(index);
            Skills.RemoveAt(index);
            Skills.Insert(index, skillToChange);
            
            SkillInventory.Remove(skillToChange);
            SkillInventory.Add(removedSkill);

            SkillInventory.Sort(new SkillComparer());
        }

        public void AddItem<T>(T item) where T : GameItem
        {
            switch (item)
            {
                case Equipment equipment:
                    AddEquip(equipment);
                    break;

                case Skill skill:
                    AddSkill(skill);
                    break;

                case Gold gold:
                    Gold.Quantity += gold.Quantity;
                    break;
            }
        }

        private void AddEquip(Equipment equipToAdd)
        {
            bool alreadyEquipped = false;

            Equipped.ForEach(equip =>
            {
                if (equip.EquipType == equipToAdd.EquipType)
                {
                    alreadyEquipped = true;
                }
            });

            if (!alreadyEquipped)
            {
                ChangeEquip(equipToAdd);
            }
            else
            {
                EquipInventory.Add(equipToAdd);
                EquipInventory.Sort(new EquipmentComparer());
            }
        }

        private void AddSkill(Skill skill)
        {
            if (Skills.Count < MaxSkillCount)
                Skills.Add(skill);
            else
            {
                SkillInventory.Add(skill);
                SkillInventory.Sort(new SkillComparer());
            }
        }

        public void SetStats(int? atk, int? def, int? mHp, int? hp, int? mMp, int? mp, int? gold)
        {
            ATK = atk ?? ATK;
            DEF = def ?? DEF;
            MaxHP = mHp ?? MaxHP;
            HP = hp ?? HP;
            MaxMP = mMp ?? MaxMP;
            MP = mp ?? MP;
            Gold.Quantity = gold ?? Gold.Quantity;
        }

        public void Regenerate(int mHpPercentage, int mMpPercentage)
        {
            HP += MaxHP * mHpPercentage / 100;
            MP += MaxMP * mMpPercentage / 100;
        }

        public void Regenerate()
            => Regenerate(100, 100);

        public void TradeItem(GameItem item, bool buying)
        {
            if (buying)
            {
                AddItem(item);
                Gold.Quantity -= item.Price;
            }
            else
            {
                if (item is Equipment equip)
                {
                    if (EquipInventory.Remove(equip))
                        Gold.Quantity += equip.Price * GameConfig.ItemPriceSellingPercentage / 100;
                }
                else if (item is Skill skill)
                {
                    if (SkillInventory.Remove(skill))
                        Gold.Quantity += skill.Price * GameConfig.ItemPriceSellingPercentage / 100;

                }
            }
        }
    }

    public class PlayerJsonConverter : JsonConverter<Player>
    {
        private static GameConfig GameConfig => ConfigManager.Instance.GameConfig;
        
        public override Player Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var player = new Player();
            int currentHP = -1, currentMP = -1;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string? propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "Equipped":
                            player.Equipped = JsonSerializer.Deserialize<List<int>>(ref reader, options)?
                                .Select(id => new Equipment(AssetLoader.GetEquip(id))).ToList() ?? [];
                            break;

                        case "EquipInventory":
                            player.EquipInventory = JsonSerializer.Deserialize<List<int>>(ref reader, options)?
                                .Select(id => new Equipment(AssetLoader.GetEquip(id))).ToList() ?? [];
                            break;

                        case "Skills":
                            player.Skills = JsonSerializer.Deserialize<List<int>>(ref reader, options)?
                                .Select(id => new Skill(AssetLoader.GetSkill(id))).ToList() ?? [];
                            break;
                            
                        case "SkillInventory":
                            player.SkillInventory = JsonSerializer.Deserialize<List<int>>(ref reader, options)?
                                .Select(id => new Skill(AssetLoader.GetSkill(id))).ToList() ?? [];
                            break;

                        case "PlayerGold":
                            player.Gold = JsonSerializer.Deserialize<Gold>(ref reader, options) ?? new Gold(GameConfig.PlayerDefaultGold);
                            break;

                        case "HP":
                            currentHP = reader.GetInt32();
                            break;

                        case "MP":
                            currentMP = reader.GetInt32();
                            break;

                        case "Name":
                            player.Name = reader.GetString() ?? "Player";
                            break;
                    }
                }
            }

            player.ATK = GameConfig.PlayerDefaultATK + player.Equipped.Sum(e => e.BonusATKPoint * GameConfig.EquipPtATKPercentage / 100);
            player.DEF = GameConfig.PlayerDefaultDEF + player.Equipped.Sum(e => e.BonusDEFPoint * GameConfig.EquipPtDEFPercentage / 100);
            player.MaxHP = GameConfig.PlayerDefaultHP + player.Equipped.Sum(e => e.BonusHPPoint * GameConfig.EquipPtHPPercentage / 100);
            player.HP = currentHP == -1 ? player.MaxHP : currentHP;
            player.MaxMP = GameConfig.PlayerDefaultMP + player.Equipped.Sum(e => e.BonusMPPoint * GameConfig.EquipPtMPPercentage / 100);
            player.MP = currentMP == -1 ? player.MaxMP : currentMP;

            return player;
        }

        public override void Write(Utf8JsonWriter writer, Player value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Equipped");
            JsonSerializer.Serialize(writer, value.Equipped.Select(e => e.ID).ToList(), options);

            writer.WritePropertyName("EquipInventory");
            JsonSerializer.Serialize(writer, value.EquipInventory.Select(e => e.ID).ToList(), options);

            writer.WritePropertyName("Skills");
            JsonSerializer.Serialize(writer, value.Skills.Select(s => s.ID).ToList(), options);

            writer.WritePropertyName("SkillInventory");
            JsonSerializer.Serialize(writer, value.SkillInventory.Select(s => s.ID).ToList(), options);

            writer.WritePropertyName("PlayerGold");
            JsonSerializer.Serialize(writer, value.Gold, options);

            writer.WritePropertyName("HP");
            JsonSerializer.Serialize(writer, value.HP, options);

            writer.WritePropertyName("MP");
            JsonSerializer.Serialize(writer, value.MP, options);

            writer.WritePropertyName("Name");
            JsonSerializer.Serialize(writer, value.Name, options);

            writer.WriteEndObject();
        }
    }
}
