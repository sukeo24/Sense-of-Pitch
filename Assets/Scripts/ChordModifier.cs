using System;

[Flags]
public enum ChordModifier
{
    None = 0,

    Seventh = 1 << 0,
    MajorSeventh = 1 << 1,

    Sus2 = 1 << 2,
    Sus4 = 1 << 3,

    Add9 = 1 << 4,

    Flat9 = 1 << 5,
    Ninth = 1 << 6,

    Eleventh = 1 << 7,
    Sharp11 = 1 << 8,

    Thirteenth = 1 << 9
}