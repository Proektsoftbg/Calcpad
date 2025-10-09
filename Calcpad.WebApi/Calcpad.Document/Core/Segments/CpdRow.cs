namespace Calcpad.Document.Core.Segments
{
    public abstract class CpdRow(uint rowIndex)
    {
        public uint RowIndex { get; protected set; } = rowIndex;
    }
}
