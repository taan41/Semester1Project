[Serializable]
abstract class Entity : Component
{
    private int
        _atk,
        _def,
        _maxHP, _HP,
        _maxMP, _MP;

    public int ATK
    {
        get => _atk;
        set
        {
            if (value < 1) value = 1;
            _atk = value;
        }
    }

    public int DEF
    {
        get => _def;
        set
        {
            if (value < 0) value = 0;
            _def = value;
        }
    }

    public int MaxHP
    {
        get => _maxHP;
        set
        {
            if (value < 1) value = 1;
            _maxHP = value;
        }
    }

    public int HP
    {
        get => _HP;
        set
        {
            if (value < 0) value = 0;
            if (value > _maxHP) value = _maxHP;
            _HP = value;
        }
    }

    public int MaxMP
    {
        get => _maxMP;
        set
        {
            if (value < 1) value = 1;
            _maxMP = value;
        }
    }

    public int MP
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

    public void Attack(Entity target)
    {
        target.HP -= ATK - target.DEF;
        HP -= target.ATK - DEF;
        MP += MaxMP * 15 / 100;
    }

    public void UseSkill(Skill skill, List<Monster> targets)
    {
        foreach (var target in targets)
            target.HP -= skill.Damage - target.DEF;

        HP += skill.Heal;
        MP -= skill.MPCost;
    }
}