[Serializable]
class GameProgress
{
    public const int MaxFloor = 3, MaxRoom = 16;

    private int _floor = 1;
    private int _room = 0;

    public int Floor
    {
        get => _floor;
        set
        {
            if (value < 0) value = 0;
            if (value > MaxFloor) value = MaxFloor;
            _floor = value;
        }
    }

    public int Room
    {
        get => _room;
        set
        {
            if (value < 0) value = 0;
            if (value > MaxRoom) value = MaxRoom;
            _room = value;
        }
    }

    public GameProgress() {}
}