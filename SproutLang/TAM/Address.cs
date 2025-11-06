namespace SproutLang.TAM;

/// <summary>
/// Represents a runtime address for variables and functions.
/// Tracks the scope level and displacement (offset) in the stack frame.
/// </summary>
public class Address
{
    public int Level { get; set; }
    public int Displacement { get; set; }

    public Address()
    {
        Level = 0;
        Displacement = 0;
    }

    public Address(int level, int displacement)
    {
        Level = level;
        Displacement = displacement;
    }

    public Address(Address other)
    {
        Level = other.Level;
        Displacement = other.Displacement;
    }

    public Address(Address other, int displacementOffset)
    {
        Level = other.Level;
        Displacement = other.Displacement + displacementOffset;
    }
}

