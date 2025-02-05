using BLL.Game.Components.Item;

namespace BLL.Game.Components.Entity
{
    [Serializable]
    public abstract class GameEntity : GameComponent
    {
        private int
            _atk, _def,
            _maxHP, _HP,
            _maxMP, _MP;

        public virtual int ATK
        {
            get => _atk;
            set
            {
                if (value < 1) value = 1;
                _atk = value;
            }
        }

        public virtual int DEF
        {
            get => _def;
            set
            {
                if (value < 0) value = 0;
                _def = value;
            }
        }

        public virtual int MaxHP
        {
            get => _maxHP;
            set
            {
                if (value < 1) value = 1;
                _maxHP = value;
            }
        }

        public virtual int HP
        {
            get => _HP;
            set
            {
                if (value < 0) value = 0;
                if (value > _maxHP) value = _maxHP;
                _HP = value;
            }
        }

        public virtual int MaxMP
        {
            get => _maxMP;
            set
            {
                if (value < 1) value = 1;
                _maxMP = value;
            }
        }

        public virtual int MP
        {
            get => _MP;
            set
            {
                if (value < 0) value = 0;
                if (value > _maxMP) value = _maxMP;
                _MP = value;
            }
        }

        public GameEntity() {}

        public GameEntity(string name, int atk, int def, int hp, int mp) : base(name)
        {
            ATK = atk;
            DEF = def;
            MaxHP = hp;
            HP = hp;
            MaxMP = mp;
            MP = mp;
        }

        public void Attack<T>(T target) where T : GameEntity
        {
            target.HP -= Math.Max(ATK - target.DEF, 1);
            HP -= Math.Max(target.ATK - DEF, 1);
            MP += MaxMP * GameConfig.EntityMPRegenPercentage / 100;
        }

        public bool UseSkill<T>(Skill skill, List<T> targets) where T : GameEntity
        {
            if (MP < skill.MPCost) return false;

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
                int typeDmgPercentage = skill.SkillType switch
                {
                    Skill.Type.Single => GameConfig.SkillTypeSinglePercentage,
                    Skill.Type.Random => GameConfig.SkillTypeRandomPercentage,
                    Skill.Type.All => GameConfig.SkillTypeAllPercentage,
                    _ => 100
                };

                foreach (var target in targets)
                    target.HP -= Math.Max(
                        skill.DamagePoint *
                        GameConfig.SkillPtDmgPercentage *
                        rarityPercentage *
                        typeDmgPercentage / 1000000 
                        - target.DEF, 1);
            }

            if (skill.HealPoint != 0)
            {
                HP += skill.HealPoint * GameConfig.SkillPtHealPercentage * (skill.HealPoint > 0 ? rarityPercentage : 100) / 10000;
            }

            if (skill.MPCost != 0)
            {
                MP -= skill.MPCost * (skill.MPCost < 0 ? rarityPercentage : 100) / 100;
            }

            return true;
        }
    }
}