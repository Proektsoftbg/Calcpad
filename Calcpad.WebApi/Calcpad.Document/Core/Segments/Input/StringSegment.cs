namespace Calcpad.Document.Core.Segments.Input
{
    public class StringSegment(string value)
    {
        public override string ToString()
        {
            return value;
        }
    }
}
