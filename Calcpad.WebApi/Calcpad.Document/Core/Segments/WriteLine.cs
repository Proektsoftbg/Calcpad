namespace Calcpad.Document.Core.Segments
{
    public enum WriteType
    {
        //For the #write and #append commands TYPE can be one of the capital letters below:

        // Yes, the matrix structure is used
        Y,

        // No, the matrix structure is not used (default)
        N
    }

    /// <summary>
    /// Formatted line object that write or append matrix to another file
    /// </summary>
    [Obsolete("Not implemented yet")]
    public class WriteLine() : CpdLine(0) { }
}
