[Serializable]
abstract class Entity : Component
{
    private int
        attack,
        maxHealth, health,
        maxMana, mana;

    public int Attack
    {
        get => attack;
        set
        {
            if (value < 1) value = 1;
            attack = value;
        }
    }

    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            if (value < 1) value = 1;
            maxHealth = value;
        }
    }

    public int Health
    {
        get => health;
        set
        {
            if (value < 0) value = 0;
            if (value > maxHealth) value = maxHealth;
            health = value;
        }
    }

    public int MaxMana
    {
        get => maxMana;
        set
        {
            if (value < 1) value = 1;
            maxMana = value;
        }
    }

    public int Mana
    {
        get => mana;
        set
        {
            if (value < 0) value = 0;
            if (value > maxMana) value = maxMana;
            mana = value;
        }
    }
}