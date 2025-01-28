using BLL.GameComponents.ItemComponents;

namespace BLL.GameComponents.EntityComponents
{
    [Serializable]
    public abstract class Entity : ComponentAbstract
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

        public Entity() {}

        public Entity(string name, int atk, int def, int hp, int mp) : base(name)
        {
            ATK = atk;
            DEF = def;
            MaxHP = hp;
            HP = hp;
            MaxMP = mp;
            MP = mp;
        }

        public void Attack<T>(T target) where T : Entity
        {
            target.HP -= Math.Max(ATK - target.DEF, 1);
            HP -= Math.Max(target.ATK - DEF, 1);
            MP += MaxMP * Config.EntityMPRegenPercentage / 100;
        }

        public bool UseSkill<T>(Skill skill, List<T> targets) where T : Entity
        {
            if (MP < skill.MPCost) return false;

            int typePercentage = skill.SkillType switch
            {
                Skill.Type.Single => Config.SkillTypeSinglePercentage,
                Skill.Type.Random => Config.SkillTypeRandomPercentage,
                Skill.Type.All => Config.SkillTypeAllPercentage,
                _ => 0
            };

            foreach (var target in targets)
                target.HP -= Math.Max(skill.DamagePoint * Config.SkillPtDamagePercentage * typePercentage / 10000 - target.DEF, 1);

            HP += skill.HealPoint * Config.SkillPtHealPercentage / 100;
            MP -= skill.MPCost;

            return true;
        }
    }
}