using System.Text.Json;

abstract class Component(string name = "Temp name")
{
    public virtual string Name { get; set; } = name;

    public virtual string ToJson()
        => JsonSerializer.Serialize(this);
}