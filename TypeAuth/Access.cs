namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// When an Action is evaluated, the Access could be one or more of the Values defined here.
    /// </summary>
    public enum Access
    {
        Read = 1,
        Write = 2,
        Delete = 3,
        Maximum = 4,
    }
}
