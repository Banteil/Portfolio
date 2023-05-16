namespace Zeus
{
    [System.Flags]
    public enum AISightMethod
    {
        Center = 1,
        Top = 2,
        Bottom = 4,
    }

    public enum AIUpdateQuality
    {
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh
    }
}
