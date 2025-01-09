using System.Text;

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

    /// <summary>
    /// Move forwards in game's progress
    /// </summary>
    /// <returns> False if having reached the end </returns>
    public bool Next()
    {
        if (Room == MaxRoom)
        {
            if (Floor == MaxFloor)
                return false;
                
            Room = 1;
            Floor++;
        }
        else
            Room++;
        
        return true;
    }

    public void Print()
    {
        StringBuilder sb = new();
        sb.Append($" Progress: {_room}/{MaxRoom} - Floor {_floor}\n");
        for (int i = 1; i <= _room; i++)
        {
            if (i == MaxRoom)
                sb.Append(" ▲ ");
            else if (i % 5 == 0)
                sb.Append(" ● ");
            else
                sb.Append(" • ");
        }
        Console.WriteLine(sb);
    }

    public override string ToString()
        => $"Room {_room}/{MaxRoom} - Floor {_floor}";
}