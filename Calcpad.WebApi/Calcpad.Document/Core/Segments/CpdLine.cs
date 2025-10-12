namespace Calcpad.Document.Core.Segments
{
    public abstract class CpdLine(uint rowIndex)
    {
        public uint RowIndex { get; protected set; } = rowIndex;
    }
}
