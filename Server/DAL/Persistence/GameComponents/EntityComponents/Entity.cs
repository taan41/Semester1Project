namespace DAL.Persistence.GameComponents.EntityComponents
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

        public Entity(Entity other) : base(other.Name)
        {
            ATK = other.ATK;
            DEF = other.DEF;
            MaxHP = other.MaxHP;
            HP = other.HP;
            MaxMP = other.MaxMP;
            MP = other.MP;
        }
    }
}