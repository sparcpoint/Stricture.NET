namespace Stricture
{
    /// <summary>The build severity a rule reports its violations at.</summary>
    public enum Severity
    {
        /// <summary>Report violations as warnings. This is the default.</summary>
        Warning = 0,

        /// <summary>Report violations as build errors.</summary>
        Error,
    }
}
